using System;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.AppModel;

namespace xnano.MVVM
{
    public class ViewModelBase
        : BindableBase, INavigationAware, IPageLifecycleAware
    {
        internal readonly INavigationService _navigationService;

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (SetProperty(ref isBusy, value))
                    IsNotBusy = !isBusy;
            }
        }

        bool isNotBusy = true;
        public bool IsNotBusy
        {
            get { return isNotBusy; }
            set
            {
                if (SetProperty(ref isNotBusy, value))
                    IsBusy = !isNotBusy;
            }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public ViewModelBase(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            // Implement in inheriting code
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
            // Implement in inheriting code
        }

        public virtual void OnNavigatingTo(INavigationParameters parameters)
        {
            // Implement in inheriting code
        }

        public virtual void OnAppearing()
        {
        }

        public virtual void OnDisappearing()
        {
        }
    }
}
