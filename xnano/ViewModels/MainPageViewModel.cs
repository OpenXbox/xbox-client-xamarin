using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Prism.Navigation;
using Prism.Services;

using xnano.Models;

namespace xnano.ViewModels
{
    public class MainPageViewModel : MVVM.ViewModelBase
    {
        readonly IPageDialogService _dialogService;
        readonly ITokenStorage _tokenStorage;

        private bool _freshAccountSet = false;

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
        public ICommand OnAppearingCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xnano.ViewModels.LoadingViewModel"/> class.
        /// </summary>
        public MainPageViewModel(INavigationService navigationService,
                                 IPageDialogService dialogService,
                                 ITokenStorage tokenStorage)
            : base(navigationService)
        {
            _dialogService = dialogService;
            _tokenStorage = tokenStorage;

            Title = "Welcome";
            Message = "Please authenticate...";

            SkipCommand = new Command(async () =>
            {
                await NavigateToConsoleListPage();
            });

            LoginCommand = new Command(async () => {
                await NavigateToAuthenticationPage();
            });

            OnAppearingCommand = new Command(async () =>
            {
                await Task.Run(async () =>
                {
                    DisableButtons();

                    IsBusy = true;
                    if (!_freshAccountSet)
                    {
                        Message = "Loading tokens from storage...";
                        var result = await _tokenStorage.LoadTokensFromStorageAsync();
                    }

                    if (_tokenStorage.IsTokenRefreshable && !_tokenStorage.IsXTokenValid)
                    {
                        Message = "Refreshing tokens...";
                        await _tokenStorage.AuthenticateXboxLive();
                        await _tokenStorage.SaveTokensToStorageAsync();
                        IsBusy = false;
                        Message = "Authentication successful!";
                        Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(
                            async () => await NavigateToConsoleListPage());
                        return;
                    }
                    Message = "Please authenticate...";
                    IsBusy = false;
                    // User can chose to authenticate or skip authentication
                    EnableButtons();
                });
            });
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

        async Task NavigateToConsoleListPage()
        {
            await _navigationService.NavigateAsync(nameof(Views.ConsoleListPage));
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            _freshAccountSet = false;
            var navMode = parameters.GetNavigationMode();
            if (navMode != NavigationMode.Back && navMode != NavigationMode.Refresh)
                return;

            if (parameters.ContainsKey("authenticationSuccess")
                && parameters.GetValue<bool>("authenticationSuccess"))
            {
                // Authentication succeded, save token to secure storage
                var account = parameters.GetValue<Xamarin.Auth.Account>("authenticationAccount");
                _tokenStorage.UpdateTokensFromAccount(account);
                _freshAccountSet = true;
            }
            else if (parameters.ContainsKey("authenticationSuccess"))
            {
                Message = "Authentication failed!\n";
                if (parameters.ContainsKey("authenticationMessage"))
                    Message += parameters.GetValue<string>("authenticationMessage");
            }
        }

        public override void OnAppearing()
        {
            OnAppearingCommand.Execute(null);
        }
    }
}
