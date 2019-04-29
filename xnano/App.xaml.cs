
using Prism;
using Prism.Ioc;
using Prism.Plugin.Popups;

using xnano.Models;
using xnano.Services;
using xnano.Views;

namespace xnano
{
    public partial class App
    {
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Views
            containerRegistry.RegisterForNavigation<Xamarin.Forms.NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<AuthenticationPage>();
            containerRegistry.RegisterForNavigation<ConsoleListPage>();
            containerRegistry.RegisterForNavigation<EnterIpAddressPopup>();
            containerRegistry.RegisterForNavigation<ConnectionPage>();
            containerRegistry.RegisterForNavigation<StreamPage>();

            containerRegistry.RegisterPopupNavigationService();

            // Singletons
            containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();

            // Instances
            /* FIXME: SecureStorage suffers from bug with API 23+
             * FIXME: This leads to "Java.Lang.UnrecoverableKeyException" when attempting to get a value
             * FIXME: Bugtracking: https://github.com/xamarin/Essentials/issues/681
             * 
             * containerRegistry.RegisterInstance<ITokenStorage>(new TokenStorage(new SecureAccountStorage("xnano")));
             */
            containerRegistry.RegisterInstance<ITokenStorage>(new TokenStorage(new PlainAccountStorage("tokens.json")));
        }
    }

}
