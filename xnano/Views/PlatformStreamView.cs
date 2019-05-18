using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace xnano.Views
{
    public class PlatformStreamView : View
    {
        public static readonly BindableProperty ViewEventCommandProperty = BindableProperty.Create(
            "ViewEventCommand",
            typeof(ICommand),
            typeof(PlatformStreamView));

        public ICommand ViewEventCommand
        {
            set { SetValue(ViewEventCommandProperty, value); }
            get { return (ICommand)GetValue(ViewEventCommandProperty); }
        }
    }
}
