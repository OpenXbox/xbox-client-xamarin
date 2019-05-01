using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Android.Graphics;
using Android.Media;
using Android.Views;

using Xamarin.Forms;

using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

using xnano.Services;

namespace xnano.Droid.Gamestream
{
    public class MediaCoreConsumer
        : Java.Lang.Object, TextureView.ISurfaceTextureListener, IConsumer, IDisposable
    {
        private bool _disposed;

        private NanoClient _nanoClient;

        private readonly VideoDecoder _video;
        private readonly VideoAssembler _videoAssembler;

        private readonly AudioDecoder _audio;
        private readonly AudioAssembler _audioAssembler;
        private readonly SmartGlass.Nano.Packets.AudioFormat _audioFormat;

        private ICommand StartStreamCommand;
        private ICommand StopStreamCommand;

        public MediaCoreConsumer(NanoClient nano)
        {
            _nanoClient = nano;

            VideoFormat videoFormat = _nanoClient.Video.AvailableFormats[0];
            _video = new VideoDecoder(MediaFormat.MimetypeVideoAvc,
                (int)videoFormat.Width, (int)videoFormat.Height);
            _videoAssembler = new VideoAssembler();

            _audioFormat = _nanoClient.Audio.AvailableFormats[0];
            _audio = new AudioDecoder(MediaFormat.MimetypeAudioAac,
                (int)_audioFormat.SampleRate, (int)_audioFormat.Channels);
            _audioAssembler = new AudioAssembler();

            StartStreamCommand = new Command(async ()
                => await _nanoClient.StartStreamAsync());

            StopStreamCommand = new Command(async ()
                => await _nanoClient.StopStreamAsync());
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _video.SetSurfaceTexture(surface);
            _nanoClient.AddConsumer(this);
            StartStreamCommand.Execute(null);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            StopStreamCommand.Execute(null);
            _nanoClient.RemoveConsumer(this);
            _video.RemoveSurfaceTexture();
            StopDecoding();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            StopDecoding();
            _video.SetSurfaceTexture(surface);
            StartDecoding();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
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
