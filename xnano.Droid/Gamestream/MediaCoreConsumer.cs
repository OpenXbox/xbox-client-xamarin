using System;
using System.Threading.Tasks;
using Android.Media;
using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;
using xnano.Services;

namespace xnano.Droid.Gamestream
{
    public class MediaCoreConsumer : IConsumer, IDisposable
    {
        private bool _disposed;

        private readonly VideoDecoder _video;
        private readonly VideoAssembler _videoAssembler;

        private readonly AudioDecoder _audio;
        private readonly AudioAssembler _audioAssembler;
        private readonly SmartGlass.Nano.Packets.AudioFormat _audioFormat;

        public MediaCoreConsumer()
        {
            VideoFormat videoFormat = SmartGlassConnection.Instance.VideoFormat;
            _video = new VideoDecoder(MediaFormat.MimetypeVideoAvc,
                (int)videoFormat.Width, (int)videoFormat.Height);
            _videoAssembler = new VideoAssembler();

            _audioFormat = SmartGlassConnection.Instance.AudioFormat;
            _audio = new AudioDecoder(MediaFormat.MimetypeAudioAac,
                (int)_audioFormat.SampleRate, (int)_audioFormat.Channels);
            _audioAssembler = new AudioAssembler();
        }

        public void OnSurfaceEventArgs(object sender, SurfaceTextureEventArgs args)
        {
            switch (args.EventType)
            {
                case SurfaceTextureEventType.TextureAvailable:
                    _video.SetSurfaceTexture(args.SurfaceTexture);
                    SmartGlassConnection.Instance.NanoClient.AddConsumer(this);
                    Task.Run(async () => await SmartGlassConnection.Instance.NanoClient.StartStreamAsync());
                    break;
                case SurfaceTextureEventType.TextureDestroyed:
                    Task.Run(async () => await SmartGlassConnection.Instance.NanoClient.StopStreamAsync());
                    SmartGlassConnection.Instance.NanoClient.RemoveConsumer(this);
                    _video.RemoveSurfaceTexture();
                    StopDecoding();
                    break;
                case SurfaceTextureEventType.TextureSizeChanged:
                    StopDecoding();
                    _video.SetSurfaceTexture(args.SurfaceTexture);
                    StartDecoding();
                    break;
            }
        }

        /// <summary>
        /// Start decoder / renderer
        /// </summary>
        /// <returns>The start.</returns>
        public bool StartDecoding()
        {
            return (_video.Initialize() && _audio.Initialize());
        }

        /// <summary>
        /// Stop decoder / renderer
        /// </summary>
        /// <returns>The stop.</returns>
        public bool StopDecoding()
        {
            return (_video.StopDecoder() && _audio.StopDecoder());
        }

        public void ConsumeAudioData(object sender, AudioDataEventArgs args)
        {
            var frame = _audioAssembler.AssembleAudioFrame(
                data: args.AudioData,
                profile: AACProfile.LC,
                samplingFreq: (int)_audioFormat.SampleRate,
                channels: (byte)_audioFormat.Channels);
            
            if (frame == null)
                return;
            
            _audio.FeedAudioData(frame);
        }

        public void ConsumeVideoData(object sender, VideoDataEventArgs args)
        {
            var frame = _videoAssembler.AssembleVideoFrame(args.VideoData);

            if (frame == null)
                return;

            _video.FeedVideoData(frame);
        }

        public void ConsumeInputFeedbackFrame(object sender, InputFrameEventArgs args)
        {
        }

        public void Dispose()
        {
            DisposeMediaCoreConsumer(true);
        }

        private void DisposeMediaCoreConsumer(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _audio.Dispose();
                _video.Dispose();
            }

            _disposed = true;
        }
    }
}
