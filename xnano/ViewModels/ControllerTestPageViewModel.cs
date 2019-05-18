using System;
using Prism.Navigation;

namespace xnano.ViewModels
{
    public class ControllerTestPageViewModel : MVVM.ViewModelBase
    {
        public ControllerTestPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }
    }
}
