using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Auth;

using XboxWebApi.Authentication;

namespace xnano.Models
{
    public interface ITokenStorage
    {
        bool IsTokenRefreshable { get; }
        bool IsXTokenValid { get; }

        AccessToken AccessToken { get; }
        RefreshToken RefreshToken { get; }
        UserToken UserToken { get; }
        XToken XToken { get; }

        Task<bool> LoadTokensFromStorageAsync();
        Task<bool> SaveTokensToStorageAsync();

        Task UpdateTokensFromAccount(Xamarin.Auth.Account account);

        Task AuthenticateXboxLive();
    }
}