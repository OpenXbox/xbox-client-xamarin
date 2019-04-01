using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;

namespace xnano.Droid
{
    [Activity(Label = "xnano.Droid", Icon = "@drawable/icon",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        bool _landscapeOrientation = false;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            MessagingCenter.Subscribe<Views.StreamPage>(this, "landscape", sender =>
            {
                LockRotation(Orientation.Horizontal);
            });

            MessagingCenter.Subscribe<Views.StreamPage>(this, "portrait", sender =>
            {
                LockRotation(Orientation.Vertical);
            });

            Rg.Plugins.Popup.Popup.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, bundle);

            LoadApplication(new App());
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
    }
}
