using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Essentials;
using Prism.Navigation;
using Prism.Services;

using xnano.Models;

namespace xnano.ViewModels
{
    public class MainPageViewModel : MVVM.ViewModelBase
    {
        readonly IPageDialogService _dialogService;
        readonly ITokenStorage _tokenStorage;

        bool _skipButtonEnabled = true;
        public bool SkipButtonEnabled
        {
            get => _skipButtonEnabled;
            set => SetProperty(ref _skipButtonEnabled, value);
        }

        bool _loginButtonEnabled = true;
        public bool LoginButtonEnabled
        {
            get => _loginButtonEnabled;
            set => SetProperty(ref _loginButtonEnabled, value);
        }

        string _message;

        /// <summary>
        /// Set by the model to the current action status
        /// </summary>
        /// <value>The message.</value>
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ICommand SkipCommand { get; }
        /// <summary>
        /// Gets the Authentication verification command.
        /// <para>Invokes either </para>
        /// </summary>
        /// <value>The ICommand</value>
        public ICommand LoginCommand { get; }
        public ICommand LoadTokensCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xnano.ViewModels.LoadingViewModel"/> class.
        /// </summary>
        public MainPageViewModel(INavigationService navigationService,
                                 IPageDialogService dialogService,
                                 ITokenStorage tokenStorage)
            : base(navigationService)
        {
            _dialogService = dialogService;
            //_tokenStorage = tokenStorage;
            _tokenStorage = new TokenStorage(new PlainAccountStorage("tokens.json", FileSystem.AppDataDirectory));

            Title = "Welcome";
            Message = "Please authenticate...";

            SkipCommand = new Command(async () =>
            {
                await SkipToConsoleListPage();
            });

            LoginCommand = new Command(async () => {
                await NavigateToAuthenticationPage();
            });

            LoadTokensCommand = new Command<Xamarin.Auth.Account>(async acc =>
            {
                await LoadTokens(acc);
            });
        }

        async Task LoadTokens(Xamarin.Auth.Account account)
        {
            IsBusy = true;
            DisableButtons();

            Message = "Loading tokens from storage...";
            bool success = await _tokenStorage.LoadTokensFromStorageAsync();

            if (account != null)
                await _tokenStorage.UpdateTokensFromAccount(account);

            if (_tokenStorage.IsTokenRefreshable && !_tokenStorage.IsXTokenValid)
            {
                Message = "Refreshing tokens...";
                await _tokenStorage.AuthenticateXboxLive();
                await _tokenStorage.SaveTokensToStorageAsync();
                IsBusy = false;
                Message = "Authentication successful!";
                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(
                    async () => await SkipToConsoleListPage());
                return;
            }
            Message = "Please authenticate...";
            IsBusy = false;
            // User can chose to authenticate or skip authentication
            EnableButtons();
        }

        void EnableButtons()
        {
            SkipButtonEnabled = true;
            LoginButtonEnabled = true;
        }

        void DisableButtons()
        {
            SkipButtonEnabled = false;
            LoginButtonEnabled = false;
        }

        async Task NavigateToAuthenticationPage()
        {
            await _navigationService.NavigateAsync(nameof(Views.AuthenticationPage));
        }

        async Task SkipToConsoleListPage()
        {
            await _navigationService.NavigateAsync($"/NavigationPage/{nameof(Views.ConsoleListPage)}");
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            Xamarin.Auth.Account account = null;
            var navMode = parameters.GetNavigationMode();
            if (navMode == Prism.Navigation.NavigationMode.Back
                || navMode == Prism.Navigation.NavigationMode.Refresh)
            {
                if (parameters.ContainsKey("authenticationSuccess")
                    && parameters.GetValue<bool>("authenticationSuccess"))
                {
                    // Authentication succeded, save token to secure storage
                    account = parameters.GetValue<Xamarin.Auth.Account>("authenticationAccount");
                }
                else if (parameters.ContainsKey("authenticationSuccess"))
                {
                    Message = "Authentication failed!\n";
                    if (parameters.ContainsKey("authenticationMessage"))
                        Message += parameters.GetValue<string>("authenticationMessage");
                }
            }

            LoadTokensCommand.Execute(account);
        }
    }
}
