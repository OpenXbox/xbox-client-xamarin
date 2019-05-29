using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Auth;
using XboxWebApi.Authentication;

namespace xnano.Models
{
    public interface IAccountStorage
    {
        Task SaveAsync(RefreshToken account);
        Task<RefreshToken> FindTokenForServiceAsync();
    }
}