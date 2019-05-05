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
        const string HardcodedUsername = "XNanoXboxLiveUser";
        private IAccountStorage AccountStorage { get; }

        private Xamarin.Auth.Account _account;
        public Xamarin.Auth.Account Account
        {
            get
            {
                return _account;
            }

            private set
            {
                _account = value;
                _account.Username = HardcodedUsername;
            }
        }

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

            _account = null;
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

        public Task UpdateTokensFromAccount(Xamarin.Auth.Account account)
        {
            // Set new account
            Account = account;

            // Initialize access/refresh tokens
            var wlResponse = CreateResponseFromAccount(Account);
            AccessToken = new AccessToken(wlResponse);
            RefreshToken = new RefreshToken(wlResponse);

            return Task.CompletedTask;
        }

        public async Task<bool> LoadTokensFromStorageAsync()
        {
            var accounts = await AccountStorage.FindAccountsForServiceAsync();

            if (accounts.Count < 1)
                return false;
            else if (accounts.Count > 1)
                throw new InvalidOperationException("Only single account storage supported");

            // Set Account loaded from storage
            await UpdateTokensFromAccount(accounts[0]);
            return true;
        }

        public async Task<bool> SaveTokensToStorageAsync()
        {
            if (Account == null)
                return false;

            await AccountStorage.SaveAsync(Account);
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

            // Update Account
            Account = CreateAccountFromResponse(wlResponse);

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
