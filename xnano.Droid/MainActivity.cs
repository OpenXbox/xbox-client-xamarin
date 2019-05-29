using System;
using Android.App;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using Android.Widget;
using Android.OS;
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
    }
}