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
    public class StreamPageRenderer : PageRenderer
    {
        private StreamPageViewModel _viewModel;
        private InputHandler _inputHandler;
        private MediaCoreConsumer _gamestreamConsumer;

        private global::Android.Widget.Button _recordButton;
        private global::Android.Widget.Button _exitButton;
        private global::Android.Views.View _view;

        private MainActivity _activity;
        private TextureView _textureView;

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
                SetupCore();

                HideButtons();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"           ERROR: ", ex.Message);
            }
        }

        void SetupUserInterface()
        {
            _activity = this.Context as MainActivity;
            if (_activity != null)
            {
                _activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)WindowFlags;
                _activity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                _view = _activity.LayoutInflater.Inflate(Resource.Layout.GamestreamLayout, this, false);
            }

            _textureView = _view.FindViewById<TextureView>(Resource.Id.textureView);

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

        void SetupCore()
        {
            _inputHandler = new InputHandler(_viewModel._nanoClient);
            _gamestreamConsumer = new Gamestream.MediaCoreConsumer(_viewModel._nanoClient);
            _textureView.SurfaceTextureListener = _gamestreamConsumer;
            _activity.RegisterGamepadListener(_inputHandler);
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

        /*
         * TODO: Reason for a 50% chance of white screen instead of video rendering?       
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
        }
        */

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

        // TODO: _activity.UnregisterGamepadListener(_inputHandler);
    }
}
