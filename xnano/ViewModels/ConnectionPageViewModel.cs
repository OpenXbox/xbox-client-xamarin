using System;
using System.Threading.Tasks;

using Prism.Navigation;
using Prism.Services;

using SmartGlass;
using SmartGlass.Nano;

namespace xnano.ViewModels
{
    public class ConnectionPageViewModel : MVVM.ViewModelBase
    {
        readonly IPageDialogService _dialogService;

        string _log;
        public string Log
        {
            get => _log;
            set => SetProperty(ref _log, value);
        }

        public ConnectionPageViewModel(INavigationService navigationService,
                                        IPageDialogService dialogService)
            : base (navigationService)
        {
            _dialogService = dialogService;

            Title = "Connection log...";
            Log = "Hello!";
        }

        void AddText(string txt)
        {
            Log += Environment.NewLine + txt;
        }

        public async Task<bool> InitializeSmartGlass(string address)
        {
            var client = await SmartGlassClient.ConnectAsync(address);
            try
            {
                AddText("Waiting for Broadcast channel to open");
                await client.BroadcastChannel.WaitForEnabledAsync(
                    TimeSpan.FromSeconds(5));

                if (!client.BroadcastChannel.Enabled)
                {
                    AddText("Broadcast channel is not enabled");
                    throw new NotSupportedException();
                }

                AddText("Starting gamestream via Broadcast channel");
                var config = Services.SmartGlassConnection.Instance.GamestreamConfiguration;
                var session = await client.BroadcastChannel
                                    .StartGamestreamAsync(config);

                AddText($"Nano is up -> {session.SessionId} TCP: {session.TcpPort} UDP: {session.UdpPort}");
                Services.SmartGlassConnection.Instance.Session = session;
                Services.SmartGlassConnection.Instance.SgClient = client;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> InitializeGamestreaming(string address)
        {
            AddText("Initializing NanoClient");
            var client = new NanoClient(address,
                Services.SmartGlassConnection.Instance.Session);

            try
            {
                AddText("Initializing Nano protocol");
                await client.InitializeProtocolAsync();

                var videoFmt = client.VideoFormats[0];
                var audioFmt = client.AudioFormats[0];

                AddText("Initializing Nano stream");
                AddText($"Video: {videoFmt.Codec} {videoFmt.Width}x{videoFmt.Height} {videoFmt.FPS}fps");
                AddText($"Audio: {audioFmt.Codec} {audioFmt.SampleRate}:{audioFmt.Channels}");
                await client.InitializeStreamAsync(audioFmt, videoFmt);

                AddText("Initialized nano successfully");
                Services.SmartGlassConnection.Instance.NanoClient = client;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task ConnectToConsole()
        {
            var address = Services.SmartGlassConnection.Instance.IPAddress;
            var success = await InitializeSmartGlass(address);
            if (!success)
            {
                AddText("SmartGlass connection failed");
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowConnectionErrorMessage());
                return;
            }

            success = await InitializeGamestreaming(address);
            if (!success)
            {
                AddText("Nano connection failed");
                Xamarin.Forms.Device.BeginInvokeOnMainThread(
                    async () => await ShowConnectionErrorMessage());
                return;
            }

            AddText("Normally a stream should appear...");
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                async () => await NavigateToStreamPage());
        }

        async Task NavigateToStreamPage()
        {
            // Navigate absolute, do not show Toolbar
            await _navigationService.NavigateAsync($"/{nameof(Views.StreamPage)}");
        }

        async Task ShowConnectionErrorMessage()
        {
            await _dialogService.DisplayAlertAsync(
                "Error", "Connecting to console failed", "OK");
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (!parameters.ContainsKey("console"))
            {
                throw new Exception("Console navigation parameter not passed");
            }

            var dev = parameters.GetValue<SmartGlass.Device>("console");
            Services.SmartGlassConnection.Instance.IPAddress = dev.Address.ToString();
        }

        public override void OnAppearing()
        {
            Task.Run(ConnectToConsole);
        }
    }
}
