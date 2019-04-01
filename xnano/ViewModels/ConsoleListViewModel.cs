using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;

using SmartGlass;
using Xamarin.Forms;
using xnano.Models;
using System.Collections.Generic;

namespace xnano.ViewModels
{
    public class ConsoleListViewModel : BaseViewModel
    {
        string _statusMessage;
        public string StatusMessage
        {
            set => SetProperty(ref _statusMessage, value);
            get => _statusMessage;
        }

        SmartGlass.Device _currentItem = null;
        public SmartGlass.Device SelectedItem
        {
            get => _currentItem;
            set
            {
                // Setting console IP address
                Services.SmartGlassConnection.Instance.IPAddress =
                    value.Address.ToString();
                SetProperty(ref _currentItem, value);
            }
        }

        public ObservableCollection<SmartGlass.Device> Items { get; set; }
        public ICommand RefreshCommand { get; }
        public ICommand AddConsoleCommand { get; }

        public event EventHandler AddConsoleRequested;
        public event EventHandler ConsoleSelected;

        public ConsoleListViewModel()
        {
            Title = "ConsoleList";
            StatusMessage = "Idle";

            Items = new ObservableCollection<SmartGlass.Device>();
            RefreshCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                IsBusy = true;

                try
                {
                    var discoveredList =
                        await Services.SmartGlassConnection.Instance.DiscoverConsoles();
                    UpdateConsoleList(discoveredList);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            });

            AddConsoleCommand = new Command(() =>
            {
                AddConsoleRequested?.Invoke(this, new EventArgs());
            });

            PropertyChanged += (sender, e) => {
                // Detect if changed property is a consolelist entry
                if (e.PropertyName == nameof(SelectedItem))
                {
                    ConsoleSelected?.Invoke(sender, e);
                }
            };
        }

        void UpdateConsoleList(IEnumerable<SmartGlass.Device> discovered)
        {
            foreach (var device in discovered)
            {
                var existingItem = Items.FirstOrDefault(x => x.LiveId == device.LiveId);
                if (existingItem != null)
                {
                    int i = Items.IndexOf(existingItem);
                    Items[i] = device;
                }
                else
                {
                    Items.Add(device);
                }
            }
        }
    }
}
