using System;
using System.Threading.Tasks;

using Prism.Navigation;
using Prism.Services;
using Xamarin.Auth;

using xnano.Models;
using xnano.Extensions;
using xnano.Services;

namespace xnano.ViewModels
{
    public class AuthenticationPageViewModel : MVVM.ViewModelBase
    {
        readonly IPageDialogService _dialogService;
        readonly IAuthenticationService _authenticationService;

        readonly Authenticator _authenticator;
        readonly Xamarin.Auth.Presenters.OAuthLoginPresenter _authPresenter;

        public AuthenticationPageViewModel(INavigationService navigationService,
                                           IPageDialogService dialogService,
                                           IAuthenticationService authenticationService)
            : base(navigationService)
        {
            _dialogService = dialogService;
            _authenticationService = authenticationService;

            _authPresenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            _authenticator = new WebRedirectAuthenticator(
                new Uri(_authenticationService.OAuthUrl),
                new Uri(_authenticationService.RedirectUrl));

            _authenticator.Completed += OnAuthCompleted;
            _authenticator.Error += OnAuthError;
        }

        void PresentLoginView()
        {
            _authPresenter.Login(_authenticator);
        }

        /// <summary>
        /// On auth completed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            if (_authenticator != null)
            {
                _authenticator.Completed -= OnAuthCompleted;
                _authenticator.Error -= OnAuthError;
            }

            var navParams = new NavigationParameters
            {
                { "authenticationSuccess", e.IsAuthenticated }
            };

            if (e.IsAuthenticated)
            {
                // Set creation timestamp to calculate expiration date later
                e.Account.SetCreationDateTime(DateTime.Now);
                // Account contains access and refresh tokens
                navParams.Add("authenticationAccount", e.Account);
            }
            else
                navParams.Add("authenticationMessage", "Authentication not completed!");

            await NavigateBackToRoot(navParams);
        }

        /// <summary>
        /// On auth error.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            if (_authenticator != null)
            {
                _authenticator.Completed -= OnAuthCompleted;
                _authenticator.Error -= OnAuthError;
            }

            var navParams = new NavigationParameters
            {
                { "authenticationSuccess", false },
                { "authenticationMessage", e.Message },
                { "authenticationException", e.Exception }
            };

            await NavigateBackToRoot(navParams);
        }

        async Task NavigateBackToRoot(INavigationParameters parameters)
        {
            await _navigationService.GoBackToRootAsync(parameters);
        }

        public override void OnAppearing()
        {
            PresentLoginView();
        }
    }
}
