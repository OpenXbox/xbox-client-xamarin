using System;
using Xunit;

using xnano.Models;
using Xamarin.Auth;

namespace xnano.Tests
{
    public class TokenStorageTests : IClassFixture<AccountStorageFixture>
    {
        Account _account;
        IAccountStorage _accountStorage;
        ITokenStorage _tokenStorage;

        public TokenStorageTests(AccountStorageFixture fixture)
        {
            _account = fixture.Account;
            _accountStorage = fixture.PlainAccountStorage;
            _tokenStorage = new TokenStorage(_accountStorage);
        }

        [Fact]
        public async void TestTokenLoading()
        {
            var success = await _tokenStorage.LoadTokensFromStorageAsync();

            Assert.True(success);
            Assert.NotNull(_tokenStorage.AccessToken);
            Assert.NotNull(_tokenStorage.RefreshToken);

            Assert.Null(_tokenStorage.UserToken);
            Assert.Null(_tokenStorage.XToken);
            Assert.False(_tokenStorage.IsTokenRefreshable);
            Assert.False(_tokenStorage.IsXTokenValid);
        }

        [Fact]
        public async void TestUpdateViaAccount()
        {
            await _tokenStorage.UpdateTokensFromAccount(_account);

            Assert.NotNull(_tokenStorage.AccessToken);
            Assert.NotNull(_tokenStorage.RefreshToken);

            Assert.Null(_tokenStorage.UserToken);
            Assert.Null(_tokenStorage.XToken);
            Assert.False(_tokenStorage.IsTokenRefreshable);
            Assert.False(_tokenStorage.IsXTokenValid);
        }
    }
}
