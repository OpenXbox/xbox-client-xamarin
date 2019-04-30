using System;
using Xamarin.Forms;
using Prism.Navigation;

using SmartGlass.Nano;

namespace xnano.ViewModels
{
    public class StreamPageViewModel : MVVM.ViewModelBase
    {
        public NanoClient _nanoClient;

        public StreamPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (!parameters.ContainsKey("nano"))
                throw new InvalidNavigationException("nano client not passed");

            _nanoClient = parameters.GetValue<NanoClient>("nano");
        }

        public override void OnAppearing()
        {
            MessagingCenter.Send(this, "landscape");
        }

        public override void OnDisappearing()
        {
            MessagingCenter.Send(this, "portrait");
        }
    }
}