
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;

using Xamarin.Essentials;

using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;
using Xamarin.Auth;
using xnano.Models;

namespace xnano.Services
{
    public class XboxLiveAuthentication
    {
        /// <summary>
        /// Singleton of this class
        /// </summary>
        static XboxLiveAuthentication _instance;
        public static XboxLiveAuthentication Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new XboxLiveAuthentication();

                return _instance;
            }
        }

        readonly string _oauthUrl;
        AuthenticationTokens Tokens;

        public bool IsTokenRefreshable => Tokens.RefreshToken != null
                                       && Tokens.RefreshToken.Valid;

        XboxLiveAuthentication()
        {
            _oauthUrl = AuthenticationService.GetWindowsLiveAuthenticationUrl();
        }

        WindowsLiveResponse CreateResponseFromAccount(Xamarin.Auth.Account account)
        {
            var nameValueCollection = account.Properties.Aggregate(new NameValueCollection(),
                (nv, current) =>
                {
                    nv.Add(current.Key, current.Value);
                    return nv;
                });
            return new WindowsLiveResponse(nameValueCollection);
        }

        Task SaveTokensToStorageAsync()
        {
            return Task.WhenAll(
                SecureStorage.SetAsync("access_token", Tokens.AccessToken.Jwt),
                SecureStorage.SetAsync("refresh_token", Tokens.RefreshToken.Jwt),
                SecureStorage.SetAsync("access_valid_until", Tokens.AccessToken.Expires.ToString()),
                SecureStorage.SetAsync("refresh_valid_until", Tokens.RefreshToken.Expires.ToString())
            );
        }

        public async Task LoadTokensFromStorageAsync()
        {
            var accessTokenJwt = await SecureStorage.GetAsync("access_token");
            var refreshTokenJwt = await SecureStorage.GetAsync("refresh_token");
            var accessTokenExpiration = await SecureStorage.GetAsync("access_token_expires");
            var refreshTokenExpiration = await SecureStorage.GetAsync("refresh_valid_until");

            if (!String.IsNullOrEmpty(accessTokenJwt) &&
                !String.IsNullOrEmpty(accessTokenExpiration))
            {
                Tokens.AccessToken = new AccessToken()
                {
                    Jwt = accessTokenJwt,
                    Expires = DateTime.Parse(accessTokenExpiration)
                };
            }

            if (!String.IsNullOrEmpty(refreshTokenJwt) &&
                !String.IsNullOrEmpty(refreshTokenExpiration))
            {
                Tokens.RefreshToken = new RefreshToken()
                {
                    Jwt = refreshTokenJwt,
                    Expires = DateTime.Parse(refreshTokenExpiration)
                };
            }
        }

        public Authenticator GetXamarinOAuthAuthenticator()
        {
            return new Xamarin.Auth.WebRedirectAuthenticator(
                new Uri(_oauthUrl),
                new Uri(WindowsLiveConstants.RedirectUrl));
        }

        public Task SetTokensFromAccount(Xamarin.Auth.Account account)
        {
            var wlResponse = CreateResponseFromAccount(account);
            Tokens.AccessToken = new AccessToken(wlResponse);
            Tokens.RefreshToken = new RefreshToken(wlResponse);
            return SaveTokensToStorageAsync();
        }

        public async Task RefreshWindowsLiveTokenAsync()
        {
            WindowsLiveResponse wlResponse =
                await AuthenticationService.RefreshLiveTokenAsync(Tokens.RefreshToken);
            Tokens.AccessToken = new AccessToken(wlResponse);
            Tokens.RefreshToken = new RefreshToken(wlResponse);
            await SaveTokensToStorageAsync();
        }

        public async Task RefreshXboxLiveTokenAsync()
        {
            var userToken = await AuthenticationService.AuthenticateXASUAsync(Tokens.AccessToken);
            Tokens.XToken = await AuthenticationService.AuthenticateXSTSAsync(userToken);
        }
    }
}