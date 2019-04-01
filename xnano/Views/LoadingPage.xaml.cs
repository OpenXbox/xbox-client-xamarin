using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using xnano.ViewModels;
using System.ComponentModel;
using Xamarin.Auth;

namespace xnano.Views
{
    public partial class LoadingPage : ContentPage
    {
        LoadingViewModel _viewModel;

        public LoadingPage()
        {
            InitializeComponent();

            _viewModel = new LoadingViewModel();
            BindingContext = _viewModel;

            _viewModel.RequireAuthentication += OnAuthenticationRequired;
            _viewModel.AuthenticationSuccess += OnAuthenticationSucceeded;
            _viewModel.AuthenticationFailure += OnAuthenticationFailed;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.VerifyAuthenticationCommand.Execute(null);
        }

        void OnAuthenticationRequired(object sender, EventArgs args)
        {
            _viewModel.ShowAuthWebviewCommand.Execute(null);
        }

        async void OnAuthenticationSucceeded(object sender, EventArgs args)
        {
            await _viewModel.Authenticate();
            // Wrap into NavigationPage to show toolbar
            await Navigation.PushModalAsync(new NavigationPage(new ConsoleListPage()));
        }

        async void OnAuthenticationFailed(object sender, EventArgs args)
        {
            await DisplayAlert("Error", "Authentication failed, try again", "OK");
            // TODO: Show error message first
            _viewModel.ShowAuthWebviewCommand.Execute(null);
        }
    }
}
