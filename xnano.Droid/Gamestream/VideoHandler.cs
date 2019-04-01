using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Media;
using Android.Views;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace xnano.Droid.Gamestream
{
    public class VideoHandler
        : MediaCodec.Callback, IDisposable
    {
        private readonly SurfaceTexture _surface;

        private SmartGlass.Nano.Packets.VideoFormat _videoFormat;
        private Queue<H264Frame> _videoFrameQueue;
        private VideoAssembler _videoAssembler;

        private MediaCodec _videoCodec;

        public VideoHandler(SurfaceTexture surface, SmartGlass.Nano.Packets.VideoFormat format)
        {
            _surface = surface;
            _videoFormat = format;

            _videoFrameQueue = new Queue<H264Frame>();
            _videoAssembler = new VideoAssembler();
        }

        public void SetupVideo(int width, int height, byte[] spsData, byte[] ppsData)
        {
            MediaFormat videoFormat = MediaFormat.CreateVideoFormat(
                mime: MediaFormat.MimetypeVideoAvc,
                width: width,
                height: height);

            /*
             * TODO: Use SPS / PPS
            videoFormat.SetByteBuffer("csd-0", Java.Nio.ByteBuffer.Wrap(spsData));
            videoFormat.SetByteBuffer("csd-1", Java.Nio.ByteBuffer.Wrap(ppsData));
            */

            videoFormat.SetInteger(MediaFormat.KeyMaxInputSize, 100000);

            _videoCodec = MediaCodec.CreateDecoderByType(
                                    MediaFormat.MimetypeVideoAvc);

            _videoCodec.SetCallback(this);
            _videoCodec.Configure(format: videoFormat,
                                  surface: new Surface(_surface),
                                  crypto: null,
                                  flags: MediaCodecConfigFlags.None);

            _videoCodec.Start();
        }

        public void ConsumeVideoData(SmartGlass.Nano.Packets.VideoData data)
        {
            H264Frame frame = _videoAssembler.AssembleVideoFrame(data);
            if (frame != null)
            {

                /* TODO: Use this.. on main thread
                if (_videoCodec == null &&
                    (frame.PrimaryType == NalUnitType.SEQUENCE_PARAMETER_SET ||
                     frame.PrimaryType == NalUnitType.PICTURE_PARAMETER_SET))
                {
                    SetupVideo((int)_videoFormat.Width,
                                   (int)_videoFormat.Height,
                                   frame.GetSpsDataPrefixed(),
                                   frame.GetPpsDataPrefixed());
                }
                if (_videoCodec != null)
                {
                    _videoFrameQueue.Enqueue(frame);
                }
                */
                _videoFrameQueue.Enqueue(frame);
            }
        }

        public override void OnError(MediaCodec codec, MediaCodec.CodecException e)
        {
            throw new NotImplementedException();
        }

        public override void OnInputBufferAvailable(MediaCodec codec, int index)
        {
            H264Frame frame;
            bool success = _videoFrameQueue.TryDequeue(out frame);

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

            // Just release outputBuffer, callback will handle rendering
            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
        }

        public void Dispose()
        {
            if (_videoCodec != null)
            {
                _videoCodec.Stop();
                _videoCodec.Release();
                _videoCodec.Dispose();
            }
            _videoFrameQueue.Clear();
        }
    }
}