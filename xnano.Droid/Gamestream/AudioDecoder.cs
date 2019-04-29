using System;
using System.Threading;
using Android.Media;
using Android.OS;
using Java.Nio;
using SmartGlass.Nano.Consumer;
using Thread = Java.Lang.Thread;

namespace xnano.Droid.Gamestream
{
    public class AudioDecoder : IDisposable
    {
        private MediaFormat _mediaFormat;
        private MediaCodec _mediaCodec;
        private AudioTrack _audioTrack;
        private Thread _encoderThread;
        private Thread _decoderThread;

        private readonly string _mimeType;
        private readonly int _sampleRate;
        private readonly int _channels;
        private readonly CircularBuffer<byte[]> _audioQueue;

        private volatile bool _initialized;
        private volatile bool _disposed;

        public static MediaFormat GetMediaFormat(string mimeType, int sampleRate, int channels)
        {
            var format = MediaFormat.CreateAudioFormat(
                mime: MediaFormat.MimetypeAudioAac,
                sampleRate: sampleRate,
                channelCount: channels);

            format.SetInteger(MediaFormat.KeyIsAdts, 0);
            format.SetInteger(MediaFormat.KeyAacProfile, (int)MediaCodecProfileType.Aacobjectlc);

            byte profile = (byte)MediaCodecProfileType.Aacobjectlc;
            byte sampleIndex = AacAdtsAssembler.GetSamplingFrequencyIndex(sampleRate);
            byte[] csd0 = new byte[2];
            csd0[0] = (byte)(((byte)profile << 3) | (sampleIndex >> 1));
            csd0[1] = (byte)((byte)((sampleIndex << 7) & 0x80) | (channels << 3));

            format.SetByteBuffer("csd-0", Java.Nio.ByteBuffer.Wrap(csd0));

            return format;
        }

        private AudioTrack GetAudioTrack()
        {

            ChannelOut channelOut = _channels == 2 ? ChannelOut.Stereo : ChannelOut.Mono;
            Encoding encoding = Encoding.Pcm16bit; ;
            int bufferSize = AudioTrack.GetMinBufferSize(_sampleRate, channelOut, encoding) * 2;

            AudioTrack audioTrack;
            AudioAttributes.Builder attributesBuilder = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Game);
            AudioFormat format = new AudioFormat.Builder()
                .SetEncoding(encoding)
                .SetSampleRate(_sampleRate)
                .SetChannelMask(channelOut)
                .Build();

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                attributesBuilder.SetFlags(AudioFlags.LowLatency);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                AudioTrack.Builder trackBuilder = new AudioTrack.Builder()
                    .SetAudioFormat(format)
                    .SetAudioAttributes(attributesBuilder.Build())
                    .SetTransferMode(AudioTrackMode.Stream)
                    .SetBufferSizeInBytes(bufferSize);

                trackBuilder.SetPerformanceMode(AudioTrackPerformanceMode.LowLatency);
                audioTrack = trackBuilder.Build();
            }
            else
            {
                audioTrack = new AudioTrack(attributesBuilder.Build(),
                    format,
                    bufferSize,
                    AudioTrackMode.Stream,
                    AudioManager.AudioSessionIdGenerate);
            }

            return audioTrack;
        }

        private Thread GetEncoderThread()
        {
            var encoderThread = new Thread(() =>
            {
                MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
                try
                {
                    while (!_disposed)
                    {
                        // Try to get an available pcm audio frame
                        int outIndex = _mediaCodec.DequeueOutputBuffer(info, 50000);
                        if (outIndex >= 0)
                        {
                            int lastIndex = outIndex;

                            // Get the last available output buffer
                            while ((outIndex = this._mediaCodec.DequeueOutputBuffer(info, 0)) >= 0)
                            {
                                this._mediaCodec.ReleaseOutputBuffer(lastIndex, false);

                                lastIndex = outIndex;
                            }

                            ByteBuffer outputBuffer = _mediaCodec.GetOutputBuffer(lastIndex);
                            _audioTrack.Write(outputBuffer, outputBuffer.Limit(), WriteMode.NonBlocking);
                            _mediaCodec.ReleaseOutputBuffer(lastIndex, false);
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // Ignore Thread got interrupted from outside
                }
            });
            encoderThread.Daemon = true;
            encoderThread.Priority = Thread.MaxPriority;
            return encoderThread;
        }

        private Thread GetDecoderThread()
        {
            var decoderThread = new Thread(() =>
            {
                try
                {
                    while (!_disposed)
                    {
                        int inputBufferIndex = _mediaCodec.DequeueInputBuffer(50000);
                        if (inputBufferIndex >= 0)
                        {
                            ByteBuffer inputBuffer = _mediaCodec.GetInputBuffer(inputBufferIndex);
                            if (inputBuffer != null)
                            {
                                byte[] sample = null;
                                do
                                {
                                    sample = _audioQueue.Size < 1 ? null : _audioQueue.Back();

                                } while (sample == null && !_disposed);
                                _audioQueue.PopBack();

                                if (sample != null)
                                {
                                    inputBuffer.Put(sample, 0, sample.Length);
                                    _mediaCodec.QueueInputBuffer(inputBufferIndex, 0, sample.Length, 0, MediaCodecBufferFlags.None);
                                }
                            }
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    // Ignore Thread got interrupted from outside
                }
            });
            decoderThread.Daemon = true;
            decoderThread.Priority = Thread.MaxPriority;
            return decoderThread;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="T:xnano.Droid.Gamestream.AudioDecoder"/> class.
        /// </summary>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channels">Channels.</param>
        public AudioDecoder(string mimeType, int sampleRate, int channels)
        {
            _initialized = false;
            _mimeType = mimeType;
            _sampleRate = sampleRate;
            _channels = channels;
            _audioQueue = new CircularBuffer<byte[]>(5);
        }

        public void FeedAudioData(AACFrame data)
        {
            if (!_initialized)
                Initialize();


            _audioQueue.PushFront(data.RawData);

            /*int inputBufferIndex = _mediaCodec.DequeueInputBuffer(50000);
            if (inputBufferIndex >= 0)
            {
                ByteBuffer inputBuffer = _mediaCodec.GetInputBuffer(inputBufferIndex);
                if (inputBuffer != null)
                {
                    inputBuffer.Put(data.RawData, 0, data.RawData.Length);
                    _mediaCodec.QueueInputBuffer(inputBufferIndex, 0, data.RawData.Length, 0, MediaCodecBufferFlags.None);
                }
            }*/
        }

        public void RequestReinit()
        {
            _initialized = false;
        }

        public bool Initialize()
        {
            _initialized = false;
            if (!StopDecoder())
                return _initialized;

            _mediaFormat = GetMediaFormat(_mimeType, _sampleRate, _channels);
            _mediaCodec = MediaCodec.CreateDecoderByType(_mimeType);
            _mediaCodec.Configure(
                format: _mediaFormat,
                surface: null,
                crypto: null,
                flags: MediaCodecConfigFlags.None);

            _audioTrack = GetAudioTrack();
            _audioTrack.Play();
            _mediaCodec.Start();


            _encoderThread = GetEncoderThread();
            _encoderThread.Start();

            _decoderThread = GetDecoderThread();
            _decoderThread.Start();

            _initialized = true;
            return _initialized;
        }

        public bool StopDecoder()
        {
            try
            {
                if (_mediaCodec != null)
                {
                    _mediaCodec.Stop();
                    _mediaCodec.Release();
                }

                if (_audioTrack != null)
                {
                    _audioTrack.Stop();
                    _audioTrack.Release();
                }

                _encoderThread?.Interrupt();
                _decoderThread?.Interrupt();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            DisposeAudioDecoder(true);
        }

        private void DisposeAudioDecoder(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _initialized = false;
                if (_mediaCodec != null)
                {
                    _mediaCodec.Stop();
                    _mediaCodec.Release();
                    _mediaCodec.Dispose();
                }

                if (_audioTrack != null)
                {
                    _audioTrack.Stop();
                    _audioTrack.Release();
                }
            }

            _initialized = false;
            _disposed = true;

            _encoderThread.Interrupt();
            _decoderThread.Interrupt();
        }
    }
}