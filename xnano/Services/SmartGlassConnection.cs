
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;

using Xamarin.Essentials;

using SmartGlass;
using SmartGlass.Nano;
using SmartGlass.Common;
using System.Collections.Generic;
using SmartGlass.Nano.Consumer;

namespace xnano.Services
{
    public class SmartGlassConnection
    {
        /* Singleton */
        static SmartGlassConnection _instance;
        public static SmartGlassConnection Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SmartGlassConnection();
                return _instance;
            }
        }

        GamestreamConfiguration GamestreamConfiguration =>
            GamestreamConfiguration.GetStandardConfig();

        SmartGlassClient _sgClient { get; set; }
        NanoClient _nanoClient { get; set; }
        GamestreamSession _session { get; set; }
        IConsumer _consumer { get; set; }

        public SmartGlass.Nano.Packets.VideoFormat VideoFormat =>
            _nanoClient.VideoFormats[0];
        public SmartGlass.Nano.Packets.AudioFormat AudioFormat =>
            _nanoClient.AudioFormats[0];

        public string IPAddress { get; set; }


        public Task<IEnumerable<SmartGlass.Device>> DiscoverConsoles()
        {
            return Device.DiscoverAsync();
        }

        public async Task<bool> InitializeSmartGlass()
        {
            _sgClient = await SmartGlassClient.ConnectAsync(IPAddress);
            try
            {
                await _sgClient.Initialize();
                await _sgClient.BroadcastChannel.WaitForEnabledAsync(
                    TimeSpan.FromSeconds(5));

                if (!_sgClient.BroadcastChannel.Enabled)
                    throw new NotSupportedException();

                _session = await _sgClient.BroadcastChannel
                                    .StartGamestreamAsync(GamestreamConfiguration);
                _sgClient.Dispose();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> InitializeGamestreaming()
        {
            _nanoClient = new NanoClient(IPAddress, _session);
            try
            {
                await _nanoClient.InitializeProtocolAsync();
                await _nanoClient.InitializeStreamAsync(
                    _nanoClient.AudioFormats[0], _nanoClient.VideoFormats[0]);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> StartStream()
        {
            try
            {
                await _nanoClient.StartStreamAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void AddConsumer(IConsumer consumer)
        {
            _consumer = consumer;
            _nanoClient.AddConsumer(consumer);
        }

        public void RemoveConsumer()
        {
            if (_consumer == null)
                return;


            _nanoClient.RemoveConsumer(_consumer);
            _consumer = null;
        }
    }
}