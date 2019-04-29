using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace xnano.Models
{
    public class SmartGlassConsoles : ObservableCollection<SmartGlass.Device>
    {
        const string PreferenceName = "consoles";

        public SmartGlassConsoles() : base()
        {
            LoadCached();
            this.CollectionChanged += (sender, e) => SaveCached();
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

        public void LoadCached()
        {
            string json = Preferences.Get(PreferenceName, null);
            if (json == null)
                return;

            var list = JsonConvert.DeserializeObject<IEnumerable<SmartGlass.Device>>(
                json, SmartGlass.Device.GetJsonSerializerSettings());

            foreach (SmartGlass.Device dev in list)
            {
                AddOrUpdateConsole(dev);
            }
        }

        void SaveCached()
        {
            IEnumerable<SmartGlass.Device> list = this;
            string json = JsonConvert.SerializeObject(
                list, SmartGlass.Device.GetJsonSerializerSettings());

            Preferences.Set(PreferenceName, json);
        }
    }
}
