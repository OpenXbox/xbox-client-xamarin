using System;
using XboxWebApi.Authentication;

namespace xnano.Models
{
    public struct AuthenticationTokens
    {
        public AccessToken AccessToken;
        public RefreshToken RefreshToken;
        public XToken XToken;
    }
}
