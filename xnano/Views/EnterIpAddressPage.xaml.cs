using System;
using System.Net;
using System.ComponentModel;
using Xamarin.Forms;

using xnano.ViewModels;
using Rg.Plugins.Popup.Services;

namespace xnano.Views
{
    public partial class EnterIpAddressPage : Rg.Plugins.Popup.Pages.PopupPage
    {
        EnterIpAddressViewModel _viewModel;

        public EnterIpAddressPage()
        {
            InitializeComponent();

            _viewModel = new EnterIpAddressViewModel();
            BindingContext = _viewModel;

            _viewModel.ConsoleAdded += OnConsoleAdded;
        }

        async void OnConsoleAdded(object sender, EventArgs args)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}

