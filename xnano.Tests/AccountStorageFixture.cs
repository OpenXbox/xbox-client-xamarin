using System;
using Xunit;

using xnano.Tests.Resources;
using xnano.Models;

namespace xnano.Tests
{
    public class AccountStorageFixture
    {
        public Xamarin.Auth.Account Account { get; private set; }
        public IAccountStorage PlainAccountStorage { get; private set; }

        public AccountStorageFixture()
        {
            Account = Newtonsoft.Json.JsonConvert.DeserializeObject<Xamarin.Auth.Account>(
                ResourcesProvider.GetString("AuthAccountSingle.json", ResourceType.Json));

            PlainAccountStorage = new PlainAccountStorage(
                "AuthAccountSingle.json",
                ResourcesProvider.GetResourceDirectory(ResourceType.Json));
        }
    }
}
