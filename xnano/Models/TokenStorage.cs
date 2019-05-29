using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;

using xnano.Extensions;

namespace xnano.Models
{
    public class TokenStorage : ITokenStorage
    {
        private IAccountStorage AccountStorage { get; }

        public bool IsTokenRefreshable => RefreshToken != null
                               && RefreshToken.Valid;
        public bool IsXTokenValid => XToken != null
                                        && XToken.Valid;

        public AccessToken AccessToken { get; private set; }
        public RefreshToken RefreshToken { get; private set; }
        public UserToken UserToken { get; private set; }
        public XToken XToken { get; private set; }

        public TokenStorage(IAccountStorage accountStorage)
        {
            AccountStorage = accountStorage;

            AccessToken = null;
            RefreshToken = null;
            UserToken = null;
            XToken = null;
        }

        public static WindowsLiveResponse CreateResponseFromAccount(Xamarin.Auth.Account account)
        {
            return new WindowsLiveResponse()
            {
                CreationTime = account.GetCreationDateTime(),
                ExpiresIn = account.GetAccessTokenExpirationSeconds(),
                AccessToken = account.GetAccessTokenJwt(),
                RefreshToken = account.GetRefreshTokenJwt(),
                TokenType = account.GetTokenType(),
                Scope = account.GetScope(),
                UserId = account.GetUserId()
            };
        }

        public static Xamarin.Auth.Account CreateAccountFromResponse(WindowsLiveResponse response)
        {
            var account = new Xamarin.Auth.Account();
            account.SetCreationDateTime(response.CreationTime);
            account.SetAccessTokenExpirationSeconds(response.ExpiresIn);
            account.SetAccessTokenJwt(response.AccessToken);
            account.SetRefreshTokenJwt(response.RefreshToken);
            account.SetTokenType(response.TokenType);
            account.SetScope(response.Scope);
            account.SetUserId(response.UserId);
            return account;
        }

        public Task UpdateToken(RefreshToken refreshToken)
        {
            RefreshToken = refreshToken;
            return Task.CompletedTask;
        }

        public async Task<bool> LoadTokenFromStorageAsync()
        {
            var token = await AccountStorage.FindTokenForServiceAsync();

            if (token == null)
                return false;

            // Set Account loaded from storage
            await UpdateToken(token);
            return true;
        }

        public async Task<bool> SaveTokenToStorageAsync()
        {
            if (RefreshToken == null)
                return false;

            await AccountStorage.SaveAsync(RefreshToken);
            return true;
        }

        public async Task AuthenticateXboxLive()
        {
            await RefreshWindowsLiveTokenAsync();
            await RefreshXboxLiveTokenAsync();
        }

        async Task RefreshWindowsLiveTokenAsync()
        {
            WindowsLiveResponse wlResponse = await AuthenticationService.RefreshLiveTokenAsync(RefreshToken);

            AccessToken = new AccessToken(wlResponse);
            RefreshToken = new RefreshToken(wlResponse);
        }

        async Task RefreshXboxLiveTokenAsync()
        {
            UserToken = await AuthenticationService.AuthenticateXASUAsync(AccessToken);
            XToken = await AuthenticationService.AuthenticateXSTSAsync(UserToken);
        }
    }
}
