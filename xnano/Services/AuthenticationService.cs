using System;
namespace xnano.Services
{
    public interface IAuthenticationService
    {
        string OAuthUrl { get; }
        string RedirectUrl { get; }
    }

    public class AuthenticationService : IAuthenticationService
    {
        public string OAuthUrl =>
            XboxWebApi.Authentication.AuthenticationService.GetWindowsLiveAuthenticationUrl();

        public string RedirectUrl =>
            XboxWebApi.Authentication.WindowsLiveConstants.RedirectUrl;
    }
}
