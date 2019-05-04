using System;
using Xunit;

using Xamarin.Auth;
using xnano.Models;
using xnano.Extensions;

namespace xnano.Tests
{
    public class AccountStorageTests : IClassFixture<AccountStorageFixture>
    {
        Account _account;

        IAccountStorage _accountStorage;
        IAccountStorage _tempAccountStorage;

        public AccountStorageTests(AccountStorageFixture fixture)
        {
            _account = fixture.Account;
            _accountStorage = fixture.PlainAccountStorage;
            _tempAccountStorage = new PlainAccountStorage(
                System.IO.Path.GetTempFileName());
        }

        [Fact]
        public async void ReadAccount()
        {
            var accounts = await _accountStorage.FindAccountsForServiceAsync();
            
            Assert.Single(accounts);

            var account = accounts[0];

            Assert.NotNull(account);
            Assert.Equal("XNanoXboxLiveUser", account.Username);
            Assert.Equal("someDummyToken", account.GetAccessTokenJwt());
            Assert.Equal("anotherDummyToken", account.GetRefreshTokenJwt());
            Assert.Equal("service::user.auth.xboxlive.com::MBI_SSL", account.GetScope());
            Assert.Equal("bearer", account.GetTokenType());
            Assert.Equal("100eff935482353", account.GetUserId());
            Assert.Equal(DateTime.Parse("2018-03-22T23:02:54.0000000Z"), account.GetCreationDateTime());
            Assert.Equal(86400, account.GetAccessTokenExpirationSeconds());
        }

        [Fact]
        public async void SaveAccount()
        {
            await _tempAccountStorage.SaveAsync(_account);
            var result = await _tempAccountStorage.FindAccountsForServiceAsync();

            Assert.Single(result);

            var account = result[0];

            Assert.NotNull(account);
            Assert.Equal(_account.Username, account.Username);
            Assert.Equal(_account.GetAccessTokenJwt(), account.GetAccessTokenJwt());
            Assert.Equal(_account.GetRefreshTokenJwt(), account.GetRefreshTokenJwt());
            Assert.Equal(_account.GetScope(), account.GetScope());
            Assert.Equal(_account.GetTokenType(), account.GetTokenType());
            Assert.Equal(_account.GetUserId(), account.GetUserId());
            Assert.Equal(_account.GetCreationDateTime(), account.GetCreationDateTime());
            Assert.Equal(_account.GetAccessTokenExpirationSeconds(), account.GetAccessTokenExpirationSeconds());
        }
    }
}