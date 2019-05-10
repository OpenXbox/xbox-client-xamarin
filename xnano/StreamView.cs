using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace xnano
{
    public class StreamView : View
    {
        public static readonly BindableProperty ViewEventCommandProperty = BindableProperty.Create(
            "ViewEventCommand",
            typeof(ICommand),
            typeof(StreamView));

        public ICommand ViewEventCommand
        {
            set { SetValue(ViewEventCommandProperty, value); }
            get { return (ICommand)GetValue(ViewEventCommandProperty); }
        }
    }
}
