using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Xamarin.Auth;

namespace xnano.Extensions
{
    public static class AuthAccountExtensions
    {
        private const string CreationTimestampProperty = "authentication_timestamp";
        private const string ExpiresInProperty = "expires_in";
        private const string AccessTokenJwtProperty = "access_token";
        private const string RefreshTokenJwtProperty = "refresh_token";
        private const string TokenTypeProperty = "token_type";
        private const string ScopeProperty = "scope";
        private const string UserIdProperty = "user_id";

        /* Additional metdata, stored alongside other properties */
        public static DateTime GetCreationDateTime(this Account account)
        {
            return DateTime.Parse(account.Properties[CreationTimestampProperty]);
        }

        public static void SetCreationDateTime(this Account account, DateTime dateTime)
        {
            account.Properties[CreationTimestampProperty] = dateTime.ToUniversalTime().ToString("o");
        }

        /* Getters for regular response properties */
        public static int GetAccessTokenExpirationSeconds(this Account account)
        {
            return int.Parse(account.Properties[ExpiresInProperty]);
        }

        public static void SetAccessTokenExpirationSeconds(this Account account, int expiresIn)
        {
            account.Properties[ExpiresInProperty] = expiresIn.ToString();
        }

        public static string GetAccessTokenJwt(this Account account)
        {
            return account.Properties[AccessTokenJwtProperty];
        }

        public static void SetAccessTokenJwt(this Account account, string jwt)
        {
            account.Properties[AccessTokenJwtProperty] = jwt;
        }

        public static string GetRefreshTokenJwt(this Account account)
        {
            return account.Properties[RefreshTokenJwtProperty];
        }

        public static void SetRefreshTokenJwt(this Account account, string jwt)
        {
            account.Properties[RefreshTokenJwtProperty] = jwt;
        }

        public static string GetTokenType(this Account account)
        {
            return account.Properties[TokenTypeProperty];
        }

        public static void SetTokenType(this Account account, string tokenType)
        {
            account.Properties[TokenTypeProperty] = tokenType;
        }

        public static string GetScope(this Account account)
        {
            return account.Properties[ScopeProperty];
        }

        public static void SetScope(this Account account, string scope)
        {
            account.Properties[ScopeProperty] = scope;
        }

        public static string GetUserId(this Account account)
        {
            return account.Properties[UserIdProperty];
        }

        public static void SetUserId(this Account account, string userId)
        {
            account.Properties[UserIdProperty] = userId;
        }
    }
}
