using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

using Prism;
using Prism.Ioc;

namespace xnano.GtkDesktop
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Gtk.Application.Init();
            Forms.Init();

            var app = new App(new GtkInitializer());
            var window = new FormsWindow();

            window.LoadApplication(app);
            window.SetApplicationTitle("xnano.GtkDesktop");

            window.Show();

            Gtk.Application.Run();          
        }

        public class GtkInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry containerRegistry)
            {
                // Register any platform specific implementations
            }
        }
    }
}
