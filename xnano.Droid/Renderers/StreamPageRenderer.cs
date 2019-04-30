using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.Widget;
using System.Threading.Tasks;

using xnano.Views;
using xnano.ViewModels;
using xnano.Droid.Renderers;
using xnano.Droid.Gamestream;

[assembly: ExportRenderer(typeof(StreamPage), typeof(StreamPageRenderer))]
namespace xnano.Droid.Renderers
{
    public class StreamPageRenderer : PageRenderer, TextureView.ISurfaceTextureListener
    {
        private StreamPageViewModel _viewModel;
        private MediaCoreConsumer _gamestreamConsumer;

        private global::Android.Widget.Button _recordButton;
        private global::Android.Widget.Button _exitButton;
        private global::Android.Views.View _view;

        private Activity _activity;
        private TextureView _textureView;
        private SurfaceTexture _surfaceTexture;

        event EventHandler<SurfaceTextureEventArgs> FireSurfaceTextureEvent;

        private const SystemUiFlags WindowFlags = SystemUiFlags.LayoutStable |
                                                  SystemUiFlags.LayoutHideNavigation |
                                                  SystemUiFlags.LayoutFullscreen |
                                                  SystemUiFlags.HideNavigation |
                                                  SystemUiFlags.Fullscreen |
                                                  SystemUiFlags.ImmersiveSticky;

        public StreamPageRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            _viewModel = Element.BindingContext as StreamPageViewModel;

            try
            {
                SetupUserInterface();
                SetupEventHandlers();

                AddView(_view);
                HideButtons();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"           ERROR: ", ex.Message);
            }
        }

        void SetupUserInterface()
        {
            _activity = this.Context as Activity;
            if (_activity != null)
            {
                _activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)WindowFlags;
                _activity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.GamestreamLayout, this, false);
            }

            _textureView = _view.FindViewById<TextureView>(Resource.Id.textureView);
            _textureView.SurfaceTextureListener = this;

            _recordButton = _view.FindViewById<global::Android.Widget.Button>(Resource.Id.recordButton);
            _exitButton = _view.FindViewById<global::Android.Widget.Button>(Resource.Id.exitButton);

            HideButtons();
        }

        void SetupEventHandlers()
        {
            _textureView.Click += TextureView_Click;
            _recordButton.Click += RecordButton_Click;
            _exitButton.Click += ExitButton_Click;
        }

        void HideButtons()
        {
            _recordButton.Enabled = false;
            _exitButton.Enabled = false;

            _recordButton.Visibility = ViewStates.Invisible;
            _exitButton.Visibility = ViewStates.Invisible;
        }

        void ShowButtons()
        {
            _recordButton.Visibility = ViewStates.Visible;
            _exitButton.Visibility = ViewStates.Visible;

            _recordButton.Enabled = true;
            _exitButton.Enabled = true;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            _view.Measure(msw, msh);
            _view.Layout(0, 0, r - l, b - t);
        }

        public override void OnWindowFocusChanged(bool hasWindowFocus)
        {
            base.OnWindowFocusChanged(hasWindowFocus);
            if (hasWindowFocus && _activity != null)
                _activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)WindowFlags;

        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            FireSurfaceTextureEvent?.Invoke(this, new SurfaceTextureEventArgs(
                SurfaceTextureEventType.TextureUpdated, surface));
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
            _surfaceTexture = surface;

            // Initialize consumer and register event handler
            _gamestreamConsumer = new Gamestream.MediaCoreConsumer(_viewModel._nanoClient);
            FireSurfaceTextureEvent += _gamestreamConsumer.OnSurfaceEventArgs;

            FireSurfaceTextureEvent?.Invoke(this, new SurfaceTextureEventArgs(
                SurfaceTextureEventType.TextureAvailable, surface));
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            FireSurfaceTextureEvent?.Invoke(this, new SurfaceTextureEventArgs(
                SurfaceTextureEventType.TextureDestroyed, surface));

            // Disconnect event handler
            FireSurfaceTextureEvent -= _gamestreamConsumer.OnSurfaceEventArgs;
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            FireSurfaceTextureEvent?.Invoke(this, new SurfaceTextureEventArgs(
                SurfaceTextureEventType.TextureSizeChanged, surface,
                width, height));
        }

        async void RecordButton_Click(object sender, EventArgs e)
        {
            await Task.CompletedTask;
        }

        async void ExitButton_Click(object sender, EventArgs e)
        {
            await Task.CompletedTask;
        }

        async void TextureView_Click(object sender, EventArgs e)
        {
            ShowButtons();
            await Task.Delay(TimeSpan.FromSeconds(3));
            HideButtons();
            await Task.CompletedTask;
        }
    }
}
