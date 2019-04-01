using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Views;
using Android.Graphics;
using Android.Widget;
using System.Threading.Tasks;

using xnano.Views;
using xnano.Droid;
using xnano.Droid.Renderers;

[assembly: ExportRenderer(typeof(StreamPage), typeof(StreamPageRenderer))]
namespace xnano.Droid.Renderers
{
    public class StreamPageRenderer : PageRenderer, TextureView.ISurfaceTextureListener
    {
        global::Android.Widget.Button recordButton;
        global::Android.Widget.Button exitButton;
        global::Android.Views.View view;

        Gamestream.MediaCoreConsumer _gamestreamConsumer;

        Activity activity;
        TextureView textureView;
        SurfaceTexture surfaceTexture;

        bool _streamInitialized;

        public StreamPageRenderer(Context context) : base(context)
        {
            _streamInitialized = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                SetupUserInterface();
                SetupEventHandlers();
                AddView(view);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"           ERROR: ", ex.Message);
            }
        }

        void SetupUserInterface()
        {
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.GamestreamLayout, this, false);

            textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
            textureView.SurfaceTextureListener = this;
        }

        void SetupEventHandlers()
        {
            recordButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.recordButton);
            recordButton.Click += RecordButton_Click;

            exitButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.exitButton);
            exitButton.Click += ExitButton_Click;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            // TODO
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
            surfaceTexture = surface;

            StartRenderingStream();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            // TODO: Release resources
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            StartRenderingStream();
        }

        async void StartRenderingStream()
        {
            if (_streamInitialized)
                return;

            _streamInitialized = true;

            _gamestreamConsumer = new Gamestream.MediaCoreConsumer(
                surfaceTexture, null, null);
            xnano.Services.SmartGlassConnection.Instance.AddConsumer(_gamestreamConsumer);
            await xnano.Services.SmartGlassConnection.Instance.StartStream();
        }

        async void RecordButton_Click(object sender, EventArgs e)
        {
        }

        async void ExitButton_Click(object sender, EventArgs e)
        {
        }
    }
}
