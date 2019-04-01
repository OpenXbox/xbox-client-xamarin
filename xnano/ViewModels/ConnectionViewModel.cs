using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using System.Net;

using SmartGlass;
using Xamarin.Forms;
using xnano.Models;
using Rg.Plugins.Popup.Services;
using SmartGlass.Common;
using SmartGlass.Nano;

namespace xnano.ViewModels
{
    public class ConnectionViewModel : BaseViewModel
    {
        string _log;
        public string Log
        {
            get => _log;
            set => SetProperty(ref _log, value);
        }

        public event EventHandler ConnectionSuccess;
        public event EventHandler ConnectionFailure;

        public ConnectionViewModel()
        {
            Title = "Connection log...";
            Log = "Hello!";
        }

        void AddText(string txt)
        {
            Log += Environment.NewLine + txt;
        }

        public async Task ConnectToConsole()
        {
            var success = await Services.SmartGlassConnection.Instance.InitializeSmartGlass();
            if (!success)
            {
                AddText("SmartGlass connection failed");
                ConnectionFailure?.Invoke(this, new EventArgs());
                return;
            }

            success = await Services.SmartGlassConnection.Instance.InitializeGamestreaming();
            if (!success)
            {
                AddText("Nano connection failed");
                ConnectionFailure?.Invoke(this, new EventArgs());
                return;
            }

            AddText("NANO is ready");
            ConnectionSuccess?.Invoke(this, new EventArgs());
        }
    }
}
