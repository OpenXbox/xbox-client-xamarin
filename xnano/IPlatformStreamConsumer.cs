using System;

using SmartGlass.Nano.Consumer;
using SmartGlass.Nano.Packets;

namespace xnano
{
    public interface IPlatformStreamConsumer : IConsumer
    {
        void InitializeVideo(VideoFormat videoFormat);
        void InitializeAudio(AudioFormat audioFormat);

        void SetSurfaceTexture(object surfaceObj);
        void RemoveSurfaceTexture(object surfaceObj);

        void StartDecoding();
        void StopDecoding();
    }
}
