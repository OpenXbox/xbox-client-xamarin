using System;
using System.Windows.Input;

using Xamarin.Forms;
using Prism.Navigation;

using SmartGlass.Nano;

namespace xnano.ViewModels
{
    public class StreamPageViewModel : MVVM.ViewModelBase
    {
        readonly IPlatformStreamConsumer _streamConsumer;

        public NanoClient _nanoClient;

        public ICommand ViewEventCommand { get; }

        private ICommand StartStreamCommand { get; }
        private ICommand StopStreamCommand { get; }

        public StreamPageViewModel(INavigationService navigationService,
                                   IPlatformStreamConsumer streamConsumer)
            : base(navigationService)
        {
            _streamConsumer = streamConsumer;

            ViewEventCommand = new Command<StreamViewEventArgs>(e =>
            {
                switch (e.EventType)
                {
                    case StreamViewEventType.VideoSurfaceCreated:
                        OnStreamViewSurfaceCreated(e);
                        break;
                    case StreamViewEventType.VideoSurfaceDestroyed:
                        OnStreamViewSurfaceDestroyed(e);
                        break;
                    case StreamViewEventType.VideoSurfaceSizeChanged:
                        OnStreamViewSurfaceSizeChanged(e);
                        break;
                    case StreamViewEventType.VideoSurfaceUpdated:
                        OnStreamViewSurfaceUpdated(e);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });

            StartStreamCommand = new Command(async ()
                => await _nanoClient.StartStreamAsync());
            StopStreamCommand = new Command(async ()
                => await _nanoClient.StopStreamAsync());
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {
            if (!parameters.ContainsKey("nano"))
                throw new InvalidNavigationException("nano client not passed");

            _nanoClient = parameters.GetValue<NanoClient>("nano");
            _streamConsumer.InitializeVideo(_nanoClient.VideoFormats[0]);
            _streamConsumer.InitializeAudio(_nanoClient.AudioFormats[0]);
        }

        public void OnStreamViewSurfaceCreated(StreamViewEventArgs args)
        {
            _streamConsumer.SetSurfaceTexture(args.SurfaceObj);
            _nanoClient.AddConsumer(_streamConsumer);
            StartStreamCommand.Execute(null);
        }

        public void OnStreamViewSurfaceDestroyed(StreamViewEventArgs args)
        {
            StopStreamCommand.Execute(null);
            _nanoClient.RemoveConsumer(_streamConsumer);
            _streamConsumer.RemoveSurfaceTexture(args.SurfaceObj);
            _streamConsumer.StopDecoding();
        }

        public void OnStreamViewSurfaceSizeChanged(StreamViewEventArgs args)
        {
            _streamConsumer.StopDecoding();
            _streamConsumer.SetSurfaceTexture(args.SurfaceObj);
            _streamConsumer.StartDecoding();
        }

        public void OnStreamViewSurfaceUpdated(StreamViewEventArgs args)
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