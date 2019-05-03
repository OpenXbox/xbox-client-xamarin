using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Essentials;
using Prism.Navigation;
using Prism.Services;

using xnano.Models;
using SmartGlass;
using SmartGlass.Nano;

namespace xnano.ViewModels
{
    public class ConsoleListPageViewModel : MVVM.ViewModelBase
    {
        readonly IPageDialogService _dialogService;

        string _statusMessage;
        public string StatusMessage
        {
            set => SetProperty(ref _statusMessage, value);
            get => _statusMessage;
        }

        public SmartGlassConsoles Consoles { get; set; }

        public ICommand LoadCachedConsoles { get; }
        public ICommand SaveCachedConsoles { get; }

        public ICommand RefreshCommand { get; }
        public ICommand AddConsoleCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ItemTappedCommand { get; }

        public ICommand PowerOnCommand { get; }
        public ICommand ConnectCommand { get; }

        public ConsoleListPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService)
            : base(navigationService)
        {
            _dialogService = dialogService;

            Title = "ConsoleList";
            StatusMessage = "Idle";

            Consoles = new SmartGlassConsoles("consoles.json", FileSystem.AppDataDirectory);

            LoadCachedConsoles = new Command(async () =>
            {
                IsBusy = true;
                await Consoles.LoadCached();
                IsBusy = false;
            });

            SaveCachedConsoles = new Command(async () =>
            {
                IsBusy = true;
                await Consoles.SaveCached();
                IsBusy = false;
            });

            RefreshCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                IsBusy = true;

                StatusMessage = "Refreshing consoles";
                await RefreshConsoles();

                StatusMessage = "Discovering new consoles";
                await DiscoverConsoles();

                StatusMessage = "Idle";
                IsBusy = false;
            });

            PowerOnCommand = new Command<SmartGlass.Device>(async dev =>
            {
                var doPoweron = await _dialogService.DisplayAlertAsync(
                    "Console unavailable", "Do you want to poweron?", "Yes", "No");

                if (!doPoweron)
                    return;

                IsBusy = true;
                StatusMessage = "Powering on console...";
                await dev.PowerOnAsync();
                StatusMessage = "Idle";
                IsBusy = false;
            });

            ConnectCommand = new Command<SmartGlass.Device>(async dev =>
            {
                IsBusy = true;
                StatusMessage = "Connecting to console...";
                var nanoClient = await Task.Run<NanoClient>(
                    async () => await ConnectToConsole(dev));

                StatusMessage = "Idle";
                IsBusy = false;

                if (nanoClient == null)
                    return;

                var navParams = new NavigationParameters
                {
                    { "nano", nanoClient }
                };

                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await NavigateToStreamPage(navParams));
            });

            ItemTappedCommand = new Command<SmartGlass.Device>(dev =>
            {
                if (dev.State == DeviceState.Available)
                {
                    ConnectCommand.Execute(dev);
                }
                else
                {
                    PowerOnCommand.Execute(dev);
                }
            });

            DeleteItemCommand = new Command<SmartGlass.Device>(DeleteConsoleEntry);
            AddConsoleCommand = new Command(async() => await ShowAddConsolePopup());

            MessagingCenter.Subscribe<EnterIpAddressPopupViewModel, IPAddress>(
                this, "addConsole", async (sender, address) =>
                {
                    IsBusy = true;
                    StatusMessage = "Trying to add console...";

                    await AddConsoleByAddress(address);

                    StatusMessage = "Idle";
                    IsBusy = false;
                });
        }

        void DeleteConsoleEntry(SmartGlass.Device device)
        {
            Consoles.Remove(device);
        }

        void UpdateConsoleList(IEnumerable<SmartGlass.Device> discovered)
        {
            foreach (var device in discovered)
            {
                Consoles.AddOrUpdateConsole(device);
            }
        }

        async Task AddConsoleByAddress(IPAddress address)
        {
            try
            {
                var dev = await Services.SmartGlassConnection.FindConsole(address);
                Consoles.AddOrUpdateConsole(dev);
            }
            catch (TimeoutException)
            {
                await _dialogService.DisplayAlertAsync(
                    "Error", $"Console {address} unreachable", "OK");
            }
        }

        async Task RefreshConsoles()
        {
            List<SmartGlass.Device> refreshedDevices = new List<SmartGlass.Device>();

            foreach(var currentDev in Consoles)
            {
                try
                {
                    var ipAddress = currentDev.Address.ToString();
                    var onlineDevice = await SmartGlass.Device.PingAsync(ipAddress);
                    refreshedDevices.Add(onlineDevice);
                }
                catch (TimeoutException)
                {
                    // Create copy of device to replace old instance
                    // => Triggers CollectionChanged on Consoles-object
                    var offlineDevice = new SmartGlass.Device(currentDev);
                    refreshedDevices.Add(offlineDevice);
                }
            }
            UpdateConsoleList(refreshedDevices);
        }

        async Task DiscoverConsoles()
        {
            try
            {
                var devs = await Services.SmartGlassConnection.DiscoverConsoles();
                UpdateConsoleList(devs);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync(
                    "Error", $"Discovery failed, error: {ex}", "OK");
            }
        }

        async Task<NanoClient> ConnectToConsole(SmartGlass.Device device)
        {
            try
            {
                var ipAddress = device.Address.ToString();
                var session = await Services.SmartGlassConnection.ConnectViaSmartGlass(ipAddress);
                return await Services.SmartGlassConnection.ConnectViaNano(ipAddress, session);
            }
            catch (Exception e)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert(e.Message));
                return null;
            }
        }

        async Task ShowErrorDisplayAlert(string message)
        {
            await _dialogService.DisplayAlertAsync("Error", message, "OK");
        }

        async Task ShowAddConsolePopup()
        {
            await _navigationService.NavigateAsync(nameof(Views.EnterIpAddressPopup));
        }

        async Task NavigateToStreamPage(INavigationParameters navParams)
        {
            await _navigationService.NavigateAsync(nameof(Views.StreamPage), navParams);
        }

        public override void OnAppearing()
        {
            LoadCachedConsoles.Execute(null);
        }

        public override void OnDisappearing()
        {
            SaveCachedConsoles.Execute(null);
        }
    }
}
