using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xamarin.Auth;
using Xamarin.Essentials;

namespace xnano.Models
{
    public class PlainAccountStorage : IAccountStorage
    {
        public string FileName { get; }

        public PlainAccountStorage(string accountFilename, string baseDir = null)
        {
            if (String.IsNullOrEmpty(baseDir))
                baseDir = String.Empty;

            FileName = Path.Combine(baseDir, accountFilename);
        }

        public Task<List<Account>> FindAccountsForServiceAsync()
        {
            var result = new List<Account>();
            try
            {
                var json = File.ReadAllText(FileName);
                var deserialized = JsonConvert.DeserializeObject<Account>(json);
                result.Add(deserialized);
            }
            catch (Exception)
            {
                Debug.WriteLine($"No accounts found in PlainAccountStorage");
            }

            return Task.FromResult(result);
        }

        public Task SaveAsync(Account account)
        {
            var json = JsonConvert.SerializeObject(account);
            File.WriteAllText(FileName, json);

            return Task.CompletedTask;
        }
    }

    public class SecureAccountStorage : IAccountStorage
    {
        public string ServiceId { get; }

        public SecureAccountStorage(string serviceId)
        {
            ServiceId = serviceId;
        }

        public async Task<List<Account>> FindAccountsForServiceAsync()
        {
            // Get the json for accounts for the service
            var json = await SecureStorage.GetAsync(ServiceId);

            try
            {
                // Try to return deserialized list of accounts
                return JsonConvert.DeserializeObject<List<Account>>(json);
            }
            catch (Exception)
            {
                Debug.WriteLine($"No accounts found for Service Id: {ServiceId}");
            }

            // If this fails, return an empty list
            return new List<Account>();
        }

        public async Task SaveAsync(Account account)
        {
            // Find existing accounts for the service
            var accounts = await FindAccountsForServiceAsync();

            // Remove existing account with Id if exists
            accounts.RemoveAll(a => a.Username == account.Username);

            // Add account we are saving
            accounts.Add(account);

            // Serialize all the accounts to javascript
            var json = JsonConvert.SerializeObject(accounts);

            // Securely save the accounts for the given service
            await SecureStorage.SetAsync(ServiceId, json);
        }

        public async Task MigrateAllAccountsAsync(IEnumerable<Account> accountStoreAccounts)
        {
            var wasMigrated = await SecureStorage.GetAsync("XamarinAuthAccountStoreMigrated");

            if (String.IsNullOrEmpty(wasMigrated) || wasMigrated == "1")
                return;

            await SecureStorage.SetAsync("XamarinAuthAccountStoreMigrated", "1");

            // Just in case, look at existing 'new' accounts
            var accounts = await FindAccountsForServiceAsync();

            foreach (var account in accountStoreAccounts)
            {
                // Check if the new storage already has this account
                // We don't want to overwrite it if it does
                if (accounts.Any(a => a.Username == account.Username))
                    continue;

                // Add the account we are migrating
                accounts.Add(account);
            }

            // Serialize all the accounts to javascript
            var json = JsonConvert.SerializeObject(accounts);

            // Securely save the accounts for the given service
            await SecureStorage.SetAsync(ServiceId, json);
        }
    }
}
