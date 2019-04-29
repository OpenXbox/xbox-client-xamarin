using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;

using Xamarin.Forms;
using Prism.Navigation;

namespace xnano.ViewModels
{
    public class EnterIpAddressPopupViewModel : MVVM.ViewModelBase
    {
        string _ipAddressInput;
        public string IpAddressInput
        {
            set => SetProperty(ref _ipAddressInput, value);
            get => _ipAddressInput;
        }

        string _errorMessage;
        public string ErrorMessage
        {
            set => SetProperty(ref _errorMessage, value);
            get => _errorMessage;
        }

        public string Message { get; }
        public ICommand AddCommand { get; }
        public ICommand CancelCommand { get; }

        public EnterIpAddressPopupViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Message = "Enter IP address";
            Title = "Add console";
            AddCommand = new Command(async () =>
            {
                if (String.IsNullOrEmpty(IpAddressInput)
                    || !IPAddress.TryParse(IpAddressInput, out IPAddress addr))
                {
                    ErrorMessage = "Please enter valid address";
                    return;
                }

                // Send to subscribed ConsoleListViewModel
                MessagingCenter.Send(this, "addConsole", addr);

                await CloseEnterIpAddressPopup();
            });

            CancelCommand = new Command(async () =>
            {
                await CloseEnterIpAddressPopup();
            });
        }

        async Task CloseEnterIpAddressPopup()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
