
using System;
using System.Net;
using Xamarin.Forms;

using xnano.Views;
using xnano.Models;

namespace xnano
{
    public partial class App : Application
    {
        public App()
        {
            NavigationPage navi = new NavigationPage(new LoadingPage());
            MainPage = navi;
        }


        protected override void OnStart()
        {
            // TODO
        }

        protected override void OnSleep()
        {
            // TODO
        }

        protected override void OnResume()
        {
            // TODO
        }
    }

}
