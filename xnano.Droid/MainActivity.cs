using System;
using Android.App;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using Android.Widget;
using Android.OS;
using Android.Views;
using Rg.Plugins.Popup;
using Xamarin.Auth.Presenters.XamarinAndroid;
using Xamarin.Forms;

namespace xnano.Droid
{
    [Activity(Label = "xnano.Droid", Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private const string WifiTag = "xnano.Droid";

        private WifiManager.WifiLock WifiLock { get; set; }

        public event Action<Keycode,KeyEvent,bool> OnButtonEvent;
        public event Action<MotionEvent> OnMotionEvent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            MessagingCenter.Subscribe<ViewModels.StreamPageViewModel>(this, "landscape", sender =>
            {
                LockRotation(Orientation.Horizontal);
            });

            MessagingCenter.Subscribe<ViewModels.StreamPageViewModel>(this, "portrait", sender =>
            {
                LockRotation(Orientation.Vertical);
            });

            Popup.Init(this, savedInstanceState);

            Forms.Init(this, savedInstanceState);
            AuthenticationConfiguration.Init(this, savedInstanceState);

            if (ApplicationContext.GetSystemService(WifiService) is WifiManager wifiManager)
            {
                WifiLock = wifiManager.CreateWifiLock(WifiMode.FullHighPerf, WifiTag);
                WifiLock.SetReferenceCounted(false);
                WifiLock.Acquire();
            }

            LoadApplication(new App());
        }

        protected override void OnDestroy()
        {
            ReleaseWifiLock();
            base.OnDestroy();
        }

        private void LockRotation(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Vertical:
                    RequestedOrientation = ScreenOrientation.Portrait;
                    break;
                case Orientation.Horizontal:
                    RequestedOrientation = ScreenOrientation.Landscape;
                    break;
            }
        }

        private void ReleaseWifiLock()
        {
            if (WifiLock != null)
            {
                WifiLock.Release();
                WifiLock = null;
            }
        }

        public void RegisterGamepadListener(IGamepadEventHandler listener)
        {
            OnButtonEvent += listener.OnButtonEvent;
            OnMotionEvent += listener.OnMotionEvent;
        }

        public void UnregisterGamepadListener(IGamepadEventHandler listener)
        {
            OnButtonEvent -= listener.OnButtonEvent;
            OnMotionEvent -= listener.OnMotionEvent;
        }

        /* Catching Gamepad actions */
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if ((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad
                || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                OnButtonEvent.Invoke(keyCode, e, true);
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if ((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad
                || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                OnButtonEvent.Invoke(keyCode, e, false);
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if ((e.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad
                || (e.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                OnMotionEvent.Invoke(e);
                return true;
            }

            return base.OnGenericMotionEvent(e);
        }
    }
}