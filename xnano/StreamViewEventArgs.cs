using System;
namespace xnano
{
    public enum StreamViewEventType
    {
        VideoSurfaceCreated,
        VideoSurfaceDestroyed,
        VideoSurfaceSizeChanged,
        VideoSurfaceUpdated
    }

    public class StreamViewEventArgs
    {
        public StreamViewEventType EventType { get; }

        public object SurfaceObj { get; }
        public int SurfaceWidth { get; }
        public int SurfaceHeight { get; }

        public StreamViewEventArgs(StreamViewEventType type, object surfaceObj,
                                   int width = 0, int height = 0)
        {
            EventType = type;
            SurfaceObj = surfaceObj;
            SurfaceWidth = width;
            SurfaceHeight = height;
        }
    }
}
