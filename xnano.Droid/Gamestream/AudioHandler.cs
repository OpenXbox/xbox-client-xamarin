using System;

using System.Collections.Generic;
using Android.Media;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace xnano.Droid.Gamestream
{
    public class AudioHandler
        : MediaCodec.Callback, IDisposable
    {
        private SmartGlass.Nano.Packets.AudioFormat _audioFormat;

        private AudioTrack _audioTrack;
        private MediaCodec _audioCodec;

        private Queue<AACFrame> _audioFrameQueue;

        public AudioHandler(SmartGlass.Nano.Packets.AudioFormat format)
        {
            _audioFormat = format;
            _audioFrameQueue = new Queue<AACFrame>();
        }

        public void SetupAudio(int sampleRate, int channels, byte[] esdsData)
        {
            _audioTrack = new AudioTrack(
                new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Media)
                    .SetContentType(AudioContentType.Music)
                    .SetFlags(AudioFlags.LowLatency)
                    .Build(),
                new Android.Media.AudioFormat.Builder()
                    .SetEncoding(Encoding.Pcm16bit)
                    .SetSampleRate(44100)
                    .SetChannelMask(ChannelOut.Stereo)
                    .Build(),
                4096,
                AudioTrackMode.Stream,
                AudioManager.AudioSessionIdGenerate);

            MediaFormat audioFormat = MediaFormat.CreateAudioFormat(
                mime: MediaFormat.MimetypeAudioAac,
                sampleRate: sampleRate,
                channelCount: channels);

            audioFormat.SetInteger(MediaFormat.KeyIsAdts, 0);
            audioFormat.SetInteger(MediaFormat.KeyAacProfile, (int)MediaCodecProfileType.Aacobjectlc);

            _audioCodec = MediaCodec.CreateDecoderByType(
                MediaFormat.MimetypeAudioAac);

            // TODO: Remove hardcoding
            byte profile = (byte)MediaCodecProfileType.Aacobjectlc;
            byte sampleIndex = AacAdtsAssembler.GetSamplingFrequencyIndex(sampleRate);
            byte[] csd0 = new byte[2];
            csd0[0] = (byte)(((byte)profile << 3) | (sampleIndex >> 1));
            csd0[1] = (byte)((byte)((sampleIndex << 7) & 0x80) | (channels << 3));
            esdsData = csd0;

            audioFormat.SetByteBuffer("csd-0", Java.Nio.ByteBuffer.Wrap(esdsData));


            _audioCodec.SetCallback(this);
            _audioCodec.Configure(
                format: audioFormat,
                surface: null,
                crypto: null,
                flags: MediaCodecConfigFlags.None);

            _audioCodec.Start();
            _audioTrack.Play();
        }

        public void ConsumeAudioData(SmartGlass.Nano.Packets.AudioData data)
        {
            AACFrame frame = AudioAssembler.AssembleAudioFrame(
                data,
                AACProfile.LC,
                (int)_audioFormat.SampleRate,
                (byte)_audioFormat.Channels);

            if (_audioCodec != null)
            {
                _audioFrameQueue.Enqueue(frame);
            }
        }

        public override void OnError(MediaCodec codec, MediaCodec.CodecException e)
        {
            System.Diagnostics.Debug.WriteLine(e);
        }

        public override void OnInputBufferAvailable(MediaCodec codec, int index)
        {
            AACFrame frame;
            bool success = _audioFrameQueue.TryDequeue(out frame);

            if (!success)
            {
                codec.QueueInputBuffer(index, 0, 0, 0, 0);
                return;
            }

            Java.Nio.ByteBuffer buffer = codec.GetInputBuffer(index);
            buffer.Put(frame.RawData);

            // tell the decoder to process the frame
            codec.QueueInputBuffer(index, 0, frame.RawData.Length, 0, 0);
        }

        public override void OnOutputBufferAvailable(MediaCodec codec, int index, MediaCodec.BufferInfo info)
        {
            Java.Nio.ByteBuffer decodedSample = codec.GetOutputBuffer(index);

            _audioTrack.Write(decodedSample, 4096, WriteMode.NonBlocking);

            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
        }

        void IDisposable.Dispose()
        {
            if (_audioCodec != null)
            {
                _audioCodec.Stop();
                _audioCodec.Release();
                _audioCodec.Dispose();
            }
            _audioFrameQueue.Clear();
        }
    }
}