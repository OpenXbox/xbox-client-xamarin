using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Xamarin.Forms;
using SmartGlass;
using xnano.ViewModels;
using Rg.Plugins.Popup.Services;

namespace xnano.Views
{
    public partial class ConsoleListPage : ContentPage
    {
        private ConsoleListViewModel _viewModel;

        public ConsoleListPage()
        {
            InitializeComponent();

            _viewModel = new ConsoleListViewModel();
            BindingContext = _viewModel;

            _viewModel.AddConsoleRequested += OnAddConsoleRequested;
            _viewModel.ConsoleSelected += OnConsoleSelected;
        }

        async void OnAddConsoleRequested(object sender, EventArgs args)
        {
            await PopupNavigation.Instance.PushAsync(new EnterIpAddressPage());
        }

        async void OnConsoleSelected(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NavigationPage(
                new ConnectionPage()));
        }
    }
}
