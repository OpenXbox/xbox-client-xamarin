using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;

using SmartGlass;
using Xamarin.Forms;
using xnano.Models;
using Rg.Plugins.Popup.Services;

namespace xnano.ViewModels
{
    public class EnterIpAddressViewModel : BaseViewModel
    {
        string _ipAddress;
        public string IpAddress
        {
            set => SetProperty(ref _ipAddress, value);
            get => _ipAddress;
        }

        string _errorMessage;
        public string ErrorMessage
        {
            set => SetProperty(ref _errorMessage, value);
            get => _errorMessage;
        }

        public string Message { get; }
        public ICommand AddCommand { get; }

        public event EventHandler ConsoleAdded;

        public EnterIpAddressViewModel()
        {
            Message = "Enter IP address";
            Title = "EnterIpAddress";
            AddCommand = new Command(() =>
            {
                if (String.IsNullOrEmpty(IpAddress))
                {
                    ErrorMessage = "Please enter valid address";
                    return;
                }

                bool success = IPAddress.TryParse(IpAddress, out IPAddress addr);
                if (!success)
                {
                    ErrorMessage = "Please enter valid address";
                    return;
                }

                // TODO: Use the address to ping console
                ConsoleAdded?.Invoke(this, new EventArgs());
                return;
            });
        }
    }
}
