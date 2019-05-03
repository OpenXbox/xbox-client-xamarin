using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Newtonsoft.Json;
using System.Text;

namespace xnano.Models
{
    public class SmartGlassConsoles : ObservableCollection<SmartGlass.Device>
    {
        public string FileName { get; }

        public SmartGlassConsoles(string accountFilename, string baseDir = null)
            : base()
        {
            if (String.IsNullOrEmpty(baseDir))
                baseDir = String.Empty;

            FileName = Path.Combine(baseDir, accountFilename);
        }

        public void AddOrUpdateConsole(SmartGlass.Device device)
        {
            var existingItem = this.FirstOrDefault(x => x.LiveId == device.LiveId);
            if (existingItem != null)
            {
                int i = this.IndexOf(existingItem);
                this[i] = device;
            }
            else
            {
                this.Add(device);
            }
        }

        public async Task<int> LoadCached()
        {
            IEnumerable<SmartGlass.Device> consoles = null;
            try
            {
                using (var fs = File.OpenRead(FileName))
                {
                    byte[] jsonBytes = new byte[fs.Length];
                    int length = await fs.ReadAsync(jsonBytes, 0, jsonBytes.Length);
                    consoles = JsonConvert.DeserializeObject<IEnumerable<SmartGlass.Device>>(
                        Encoding.UTF8.GetString(jsonBytes),
                        SmartGlass.Device.GetJsonSerializerSettings());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed loading cached consoles from {FileName}, error: {e}");
                return 0;
            }

            foreach (SmartGlass.Device dev in consoles)
            {
                AddOrUpdateConsole(dev);
            }

            return consoles.Count();
        }

        public async Task<bool> SaveCached()
        {
            IEnumerable<SmartGlass.Device> list = this;

            try
            {
                string json = JsonConvert.SerializeObject(
                    list, SmartGlass.Device.GetJsonSerializerSettings());

                using (var fs = File.OpenWrite(FileName))
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed saving cached consoles, error: {e}");
                return false;
            }

            return true;
        }
    }
}
