using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using xnano.ViewModels;

namespace xnano.Views
{
    public partial class ConnectionPage : ContentPage
    {
        ConnectionViewModel _viewModel;

        public ConnectionPage()
        {
            InitializeComponent();

            _viewModel = new ConnectionViewModel();
            BindingContext = _viewModel;

            _viewModel.ConnectionSuccess += async (sender, e) => {
                await Navigation.PushAsync(new StreamPage());
            };

            _viewModel.ConnectionFailure += async (sender, e) => {
                await DisplayAlert("Error", "Connecting to console failed", "OK");
                await Navigation.PopAsync();
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(async () => await _viewModel.ConnectToConsole());
        }
    }
}
