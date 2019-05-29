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

        Task<bool> LoadTokenFromStorageAsync();
        Task<bool> SaveTokenToStorageAsync();

        Task UpdateToken(RefreshToken refreshToken);

        Task AuthenticateXboxLive();
    }
}