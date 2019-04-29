using System;
using Android.Graphics;

namespace xnano.Droid.Gamestream
{
    public enum SurfaceTextureEventType
    {
        TextureUpdated,
        TextureSizeChanged,
        TextureAvailable,
        TextureDestroyed,
        ExitPage
    }

    public class SurfaceTextureEventArgs : EventArgs
    {
        public readonly SurfaceTextureEventType EventType;
        public readonly SurfaceTexture SurfaceTexture;
        public readonly int TextureWidth = 0;
        public readonly int TextureHeight = 0;

        public SurfaceTextureEventArgs(SurfaceTextureEventType type,
            SurfaceTexture surface, int textureWidth=0, int textureHeight=0)
        {
            EventType = type;
            SurfaceTexture = surface;
            TextureWidth = textureWidth;
            TextureHeight = textureHeight;
        }
    }
}
