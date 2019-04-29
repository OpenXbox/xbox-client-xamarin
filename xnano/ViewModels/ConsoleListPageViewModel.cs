using System;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Prism.Navigation;
using Prism.Services;

using xnano.Models;

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

            ItemTappedCommand = new Command<SmartGlass.Device>(async (selectedItem) =>
            {
                await NavigateToConnectionPage(selectedItem);
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

        async Task ShowAddConsolePopup()
        {
            await _navigationService.NavigateAsync(nameof(Views.EnterIpAddressPopup));
        }

        async Task NavigateToConnectionPage(SmartGlass.Device device)
        {
            var parameters = new NavigationParameters();
            parameters.Add("console", device);
            await _navigationService.NavigateAsync(nameof(Views.ConnectionPage), parameters);
        }
    }
}
