using System;
using System.Collections.Generic;
using System.Diagnostics;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Java.Lang;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace xnano.Droid.Gamestream
{
    public class VideoDecoder
        : MediaCodec.Callback, IDisposable
    {
        private bool _disposed = false;

        private readonly CircularBuffer<byte[]> _videoQueue;
        private readonly MediaCodec _mediaCodec;
        private readonly string _mimeType;
        private readonly int _videoWidth;
        private readonly int _videoHeight;

        private SurfaceTexture _surfaceTexture;
        private Surface _surface;
        private MediaFormat _mediaFormat; // Set in Initialize()
        private bool Initialized { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xnano.Droid.Gamestream.VideoHandler"/> class.
        /// </summary>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="videoWidth">Video width.</param>
        /// <param name="videoHeight">Video height.</param>
        public VideoDecoder(string mimeType, int videoWidth, int videoHeight)
        {
            _mimeType = mimeType;
            _videoWidth = videoWidth;
            _videoHeight = videoHeight;
            Initialized = false;

            // Buffer maximum of 10 frames
            _videoQueue = new CircularBuffer<byte[]>(10);
            _mediaCodec = MediaCodec.CreateDecoderByType(_mimeType);

            _surface = null;
            _surfaceTexture = null;
        }

        /// <summary>
        /// Sets the surface texture.
        /// </summary>
        /// <param name="surfaceTexture">Surface texture.</param>
        public void SetSurfaceTexture(SurfaceTexture surfaceTexture)
        {
            _surfaceTexture = surfaceTexture;
            _surface = new Surface(_surfaceTexture);
        }

        public void RemoveSurfaceTexture()
        {
            _surface = null;
            _surfaceTexture = null;

            StopDecoder();
            Initialized = false;
        }

        public void FeedVideoData(H264Frame data)
        {
            if (Initialized)
            {
                _videoQueue.PushFront(data.RawData);
            }
            else if (data.ContainsIFrame)
            {
                Initialize();
                _videoQueue.PushFront(data.RawData);
            }
        }

        bool SetCodecSpecificData(byte[] sps, byte[] pps)
        {
            if (!Initialized)
                return false;

            _mediaFormat.SetByteBuffer("csd-0", Java.Nio.ByteBuffer.Wrap(sps));
            _mediaFormat.SetByteBuffer("csd-1", Java.Nio.ByteBuffer.Wrap(pps));
            return true;
        }

        public void RequestReinit()
        {
            Initialized = false;
        }

        /// <summary>
        /// Initialize this instance.
        /// First, stop decoder and check if surface exists.
        /// Then configure MediaFormat and MediaCodec and start it.
        /// </summary>
        /// <returns>The initialize.</returns>
        public bool Initialize()
        {
            Initialized = false;
            if (!StopDecoder() || _surface == null)
                return Initialized;

            _mediaFormat = GetMediaFormat(_mimeType, _videoWidth, _videoHeight);
            _mediaFormat.SetInteger(MediaFormat.KeyMaxWidth, _videoWidth);
            _mediaFormat.SetInteger(MediaFormat.KeyMaxHeight, _videoHeight);


            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                _mediaFormat.SetInteger(MediaFormat.KeyOperatingRate, Short.MaxValue);
            }

            _mediaCodec.Configure(
                format: _mediaFormat,
                surface: _surface,
                crypto: null,
                flags: MediaCodecConfigFlags.None);
            _mediaCodec.SetVideoScalingMode(VideoScalingMode.ScaleToFit);

            _mediaCodec.SetCallback(this);
            _mediaCodec.Start();
            Initialized = true;
            return Initialized;
        }

        /// <summary>
        /// Stops the decoder
        /// </summary>
        /// <returns><c>true</c>, if decoder was stoped, <c>false</c> otherwise.</returns>
        public bool StopDecoder()
        {
            try
            {
                if (_mediaCodec != null)
                {
                    _mediaCodec.Stop();
                    _mediaCodec.Reset();
                }

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public override void OnError(MediaCodec codec, MediaCodec.CodecException e)
        {
            System.Diagnostics.Debug.WriteLine($"Error for codec: {codec.Name}, msg: {e}");
        }

        /// <summary>
        /// Called when MediaCodec wants a new frame to decode
        /// </summary>
        /// <param name="codec">Codec.</param>
        /// <param name="index">Index.</param>
        public override void OnInputBufferAvailable(MediaCodec codec, int index)
        {
            if (_videoQueue.Size < 1)
            {
                // FIXME: Is it proper to enqueue an empty
                // buffer like this?
                codec.QueueInputBuffer(index, 0, 0, 0, MediaCodecBufferFlags.None);
                return;
            }

            var data = _videoQueue.Back();

            _videoQueue.PopBack();
            if (data != null)
            {
                // Get pre-allocated buffer from MediaCodec
                Java.Nio.ByteBuffer buffer = codec.GetInputBuffer(index);

                // Stuff in our raw framedata
                buffer.Put(data);

                // Tell the decoder to process the frame
                codec.QueueInputBuffer(index, 0, data.Length, 0, MediaCodecBufferFlags.None);
            }
        }

        /// <summary>
        /// Called when MediaCodec has a decoded frame ready
        /// </summary>
        /// <param name="codec">Codec.</param>
        /// <param name="index">Index.</param>
        /// <param name="info">Info.</param>
        public override void OnOutputBufferAvailable(MediaCodec codec, int index, MediaCodec.BufferInfo info)
        {
            // Get decoded framedata from MediaCodec
            Java.Nio.ByteBuffer decodedSample = codec.GetOutputBuffer(index);

            // Just release outputBuffer, callback will handle rendering
            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
            System.Diagnostics.Debug.WriteLine($"OutputFormat changed for Codec: {codec.Name}");
        }

        private static MediaFormat GetMediaFormat(string mimeType, int videoWidth, int videoHeight)
        {
            var format = MediaFormat.CreateVideoFormat(
                mime: mimeType,
                width: videoWidth,
                height: videoHeight);
            format.SetInteger(MediaFormat.KeyMaxInputSize, 100000);
            return format;
        }

        public new void Dispose()
        {
            base.Dispose(true);
            DisposeVideoDecoder(true);
        }

        private void DisposeVideoDecoder(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_mediaCodec != null)
                {
                    _mediaCodec.Stop();
                    _mediaCodec.Release();
                    _mediaCodec.Dispose();
                }
            }

            Initialized = false;
            _disposed = true;
        }
    }
}