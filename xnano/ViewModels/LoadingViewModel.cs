using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;

using Xamarin.Auth;
using Xamarin.Forms;
using xnano.Services;

namespace xnano.ViewModels
{
    public class LoadingViewModel : BaseViewModel
    {
        Xamarin.Auth.Authenticator _authenticator;

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

        /// <summary>
        /// Gets the Authentication verification command.
        /// <para>Invokes either </para>
        /// </summary>
        /// <value>The ICommand</value>
        public ICommand VerifyAuthenticationCommand { get; }
        /// <summary>
        /// Gets the show auth webview command.
        /// </summary>
        /// <value>The show auth webview command.</value>
        public ICommand ShowAuthWebviewCommand { get; }

        /// <summary>
        /// Occurs when require authentication.
        /// </summary>
        public event EventHandler RequireAuthentication;
        /// <summary>
        /// Occurs when on success.
        /// </summary>
        public event EventHandler AuthenticationSuccess;
        /// <summary>
        /// Occurs when on failure.
        /// </summary>
        public event EventHandler AuthenticationFailure;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xnano.ViewModels.LoadingViewModel"/> class.
        /// </summary>
        public LoadingViewModel()
        {
            _authenticator = XboxLiveAuthentication.Instance.GetXamarinOAuthAuthenticator();

            Title = "Loading";
            Message = "Please wait ...";
            IsBusy = true;

            VerifyAuthenticationCommand = new Command(async () => {
                IsBusy = true;

                await XboxLiveAuthentication.Instance.LoadTokensFromStorageAsync();

                if (!XboxLiveAuthentication.Instance.IsTokenRefreshable)
                {
                    RequireAuthentication?.Invoke(this, new EventArgs());
                }
                else
                {
                    AuthenticationSuccess?.Invoke(this, new EventArgs());
                }
            });

            ShowAuthWebviewCommand = new Command(() => {
                _authenticator.ShowErrors = true;
                _authenticator.AllowCancel = false;

                _authenticator.Completed += OnAuthCompleted;
                _authenticator.Error += OnAuthError;

                var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
                presenter.Login(_authenticator);
            });
        }

        /// <summary>
        /// Ons the auth completed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            if (_authenticator != null)
            {
                _authenticator.Completed -= OnAuthCompleted;
                _authenticator.Error -= OnAuthError;
            }

            if (e.IsAuthenticated)
            {
                XboxLiveAuthentication.Instance.SetTokensFromAccount(e.Account)
                    .GetAwaiter().GetResult();

                AuthenticationSuccess?.Invoke(this, e);
            }
            else
            {
                AuthenticationFailure?.Invoke(this, e);
            }

        }

        /// <summary>
        /// Ons the auth error.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            if (_authenticator != null)
            {
                _authenticator.Completed -= OnAuthCompleted;
                _authenticator.Error -= OnAuthError;
            }

            AuthenticationFailure?.Invoke(this, e);
            Debug.WriteLine("Authentication error: " + e.Message);
        }

        /// <summary>
        /// Authenticate this instance.
        /// </summary>
        /// <returns>The authenticate.</returns>
        public async Task Authenticate()
        {
            Message = "Authenticating with Xbox Live";
            await XboxLiveAuthentication.Instance.RefreshWindowsLiveTokenAsync();
            await XboxLiveAuthentication.Instance.RefreshXboxLiveTokenAsync();
        }
    }
}
