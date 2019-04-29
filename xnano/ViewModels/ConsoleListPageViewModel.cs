using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
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

            Consoles = new SmartGlassConsoles();
            RefreshCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                StatusMessage = "Refreshing consoles";
                IsBusy = true;

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
                var success = await Task.Run<bool>(
                    async () => await ConnectToConsole(dev));

                StatusMessage = "Idle";
                IsBusy = false;

                if (!success)
                    return;

                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await NavigateToStreamPage(dev));
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
                var dev = await Services.SmartGlassConnection.Instance.FindConsole(address);
                Consoles.AddOrUpdateConsole(dev);
            }
            catch (TimeoutException)
            {
                await _dialogService.DisplayAlertAsync(
                    "Error", $"Console {address} unreachable", "OK");
            }
        }

        async Task DiscoverConsoles()
        {
            try
            {
                var devs = await Services.SmartGlassConnection.Instance.DiscoverConsoles();
                UpdateConsoleList(devs);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync(
                    "Error", $"Discovery failed, error: {ex}", "OK");
            }
        }

        async Task<bool> InitializeSmartGlass(string address)
        {
            string lastStatus = String.Empty;
            try
            {
                lastStatus = "Connecting to console";
                var client = await SmartGlassClient.ConnectAsync(address);

                lastStatus = "Waiting for Broadcast channel";
                await client.BroadcastChannel.WaitForEnabledAsync(
                    TimeSpan.FromSeconds(5));

                if (!client.BroadcastChannel.Enabled)
                {
                    throw new NotSupportedException("Broadcast channel is not enabled");
                }

                lastStatus = "Starting gamestream via Broadcast channel";
                var config = Services.SmartGlassConnection.Instance.GamestreamConfiguration;
                var session = await client.BroadcastChannel
                                    .StartGamestreamAsync(config);

                Services.SmartGlassConnection.Instance.Session = session;
                Services.SmartGlassConnection.Instance.SgClient = client;
            }
            catch (TimeoutException)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert($"{lastStatus} timed out!"));
                return false;
            }
            catch (NotSupportedException e)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert($"{e.Message}"));
                return false;
            }
            catch (Exception)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert($"Unknown error: {lastStatus}"));
                return false;
            }

            return true;
        }

        async Task<bool> InitializeGamestreaming(string address)
        {
            string lastStatus = String.Empty;
            var client = new NanoClient(address,
                Services.SmartGlassConnection.Instance.Session);

            try
            {
                lastStatus = "Initializing Nano protocol";
                await client.InitializeProtocolAsync();

                var videoFmt = client.VideoFormats[0];
                var audioFmt = client.AudioFormats[0];

                lastStatus = "Initializing Nano stream";
                await client.InitializeStreamAsync(audioFmt, videoFmt);

                Services.SmartGlassConnection.Instance.NanoClient = client;
            }
            catch (TimeoutException)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert($"{lastStatus} timed out!"));
                return false;
            }
            catch (Exception)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowErrorDisplayAlert($"Unknown error: {lastStatus}"));
                return false;
            }

            return true;
        }

        async Task<bool> ConnectToConsole(SmartGlass.Device device)
        {
            var address = device.Address.ToString();
            if(!await InitializeSmartGlass(address))
            {
                return false;
            }

            if (!await InitializeGamestreaming(address))
            {
                return false;
            }

            return true;
        }

        async Task ShowErrorDisplayAlert(string message)
        {
            await _dialogService.DisplayAlertAsync("Error", message, "OK");
        }

        async Task ShowAddConsolePopup()
        {
            await _navigationService.NavigateAsync(nameof(Views.EnterIpAddressPopup));
        }

        async Task NavigateToStreamPage(SmartGlass.Device device)
        {
            Services.SmartGlassConnection.Instance.IPAddress = device.Address.ToString();
            await _navigationService.NavigateAsync(nameof(Views.StreamPage));
        }
    }
}
