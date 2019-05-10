using System;
using Android.Graphics;
using Android.Media;

using Xamarin.Forms;

using SmartGlass.Nano;
using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

[assembly: Dependency(typeof(xnano.Droid.Gamestream.MediaCoreConsumer))]
namespace xnano.Droid.Gamestream
{
    public class MediaCoreConsumer
        : IPlatformStreamConsumer, IDisposable
    {
        private bool _disposed;

        private readonly VideoAssembler _videoAssembler;
        private readonly AudioAssembler _audioAssembler;

        private VideoDecoder _video;
        private AudioDecoder _audio;
        private SmartGlass.Nano.Packets.VideoFormat _videoFormat;
        private SmartGlass.Nano.Packets.AudioFormat _audioFormat;

        public MediaCoreConsumer()
        {
            _videoAssembler = new VideoAssembler();
            _audioAssembler = new AudioAssembler();
        }

        /// <summary>
        /// Start decoder / renderer
        /// </summary>
        /// <returns>The start.</returns>
        public void StartDecoding()
        {
            _video.Initialize();
            _audio.Initialize();
        }

        /// <summary>
        /// Stop decoder / renderer
        /// </summary>
        /// <returns>The stop.</returns>
        public void StopDecoding()
        {
            _video.StopDecoder();
            _audio.StopDecoder();
        }

        public void InitializeVideo(VideoFormat videoFormat)
        {
            _videoFormat = videoFormat;
            _video = new VideoDecoder(MediaFormat.MimetypeVideoAvc,
                (int)videoFormat.Width, (int)videoFormat.Height);
        }

        public void InitializeAudio(SmartGlass.Nano.Packets.AudioFormat audioFormat)
        {
            _audioFormat = audioFormat;
            _audio = new AudioDecoder(MediaFormat.MimetypeAudioAac,
                (int)_audioFormat.SampleRate, (int)_audioFormat.Channels);
        }

        public void SetSurfaceTexture(object surfaceObj)
        {
            _video.SetSurfaceTexture((SurfaceTexture)surfaceObj);
        }

        public void RemoveSurfaceTexture(object surfaceObj)
        {
            _video.RemoveSurfaceTexture();
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
