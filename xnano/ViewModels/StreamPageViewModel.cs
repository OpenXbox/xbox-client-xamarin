using System;
using Xamarin.Forms;
using Prism.Navigation;


namespace xnano.ViewModels
{
    public class StreamPageViewModel : MVVM.ViewModelBase
    {
        public StreamPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
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