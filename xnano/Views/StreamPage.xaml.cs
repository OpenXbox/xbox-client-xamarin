using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace xnano.Views
{
    public partial class StreamPage : ContentPage
    {
        public StreamPage()
        {
            // A custom renderer is used to display this page
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(this, "landscape");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send(this, "portrait");
        }
    }
}
