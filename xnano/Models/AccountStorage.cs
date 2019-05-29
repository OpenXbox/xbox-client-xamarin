using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xamarin.Essentials;
using XboxWebApi.Authentication;

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

        public Task<RefreshToken> FindTokenForServiceAsync()
        {
            try
            {
                var json = File.ReadAllText(FileName);
                var deserialized = JsonConvert.DeserializeObject<RefreshToken>(json);
                return Task.FromResult(deserialized);
            }
            catch (Exception)
            {
                Debug.WriteLine($"No token found in PlainAccountStorage");
            }

            return Task.FromResult<RefreshToken>(null);
        }

        public Task SaveAsync(RefreshToken token)
        {
            var json = JsonConvert.SerializeObject(token);
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

        public async Task<RefreshToken> FindTokenForServiceAsync()
        {
            // Get the json for token for the service
            var json = await SecureStorage.GetAsync(ServiceId);

            try
            {
                // Try to return deserialized token
                return JsonConvert.DeserializeObject<RefreshToken>(json);
            }
            catch (Exception)
            {
                Debug.WriteLine($"No accounts found for Service Id: {ServiceId}");
            }

            // If this fails, return null
            return null;
        }

        public async Task SaveAsync(RefreshToken token)
        {
            // Serialize the token to javascript
            var json = JsonConvert.SerializeObject(token);

            // Securely save the token for the given service
            await SecureStorage.SetAsync(ServiceId, json);
        }
    }
}
