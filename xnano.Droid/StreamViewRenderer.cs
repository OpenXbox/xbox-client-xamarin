using System;
using Android.Content;
using Android.Graphics;
using Android.Views;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(xnano.StreamView), typeof(xnano.Droid.StreamViewRenderer))]
namespace xnano.Droid
{
    public class StreamViewRenderer
        : ViewRenderer<xnano.StreamView, TextureView>, TextureView.ISurfaceTextureListener
    {
        TextureView _textureView;

        public StreamViewRenderer(Context context) : base(context)
        {
        }

        private void OnViewEvent(StreamViewEventArgs args)
        {
            Element?.ViewEventCommand?.Execute(args);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<xnano.StreamView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                _textureView = new TextureView(Context);
                SetNativeControl(_textureView);
            }

            if (e.OldElement != null)
            {
                _textureView.SurfaceTextureListener = null;
            }
            if (e.NewElement != null)
            {
                _textureView.SurfaceTextureListener = this;
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            int height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            SetMeasuredDimension(width, height);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            _textureView.Measure(msw, msh);
            _textureView.Layout(0, 0, r - l, b - t);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            OnViewEvent(new StreamViewEventArgs(
                StreamViewEventType.VideoSurfaceCreated, surface, width, height));        
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            OnViewEvent(new StreamViewEventArgs(
                StreamViewEventType.VideoSurfaceDestroyed, surface));

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            OnViewEvent(new StreamViewEventArgs(
                StreamViewEventType.VideoSurfaceSizeChanged, surface, width, height));
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            OnViewEvent(new StreamViewEventArgs(
                StreamViewEventType.VideoSurfaceUpdated, surface));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cleanup ViewRenderer
            }
            base.Dispose(disposing);
        }
    }
}
