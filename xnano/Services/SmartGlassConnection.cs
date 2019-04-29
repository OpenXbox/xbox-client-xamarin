
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
using System.Net;

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

        public GamestreamConfiguration GamestreamConfiguration =>
            GamestreamConfiguration.GetStandardConfig();

        public SmartGlassClient SgClient { get; set; }
        public NanoClient NanoClient { get; set; }
        public GamestreamSession Session { get; set; }

        public SmartGlass.Nano.Packets.VideoFormat VideoFormat =>
            NanoClient.VideoFormats[0];
        public SmartGlass.Nano.Packets.AudioFormat AudioFormat =>
            NanoClient.AudioFormats[0];

        public string IPAddress { get; set; }


        public Task<IEnumerable<SmartGlass.Device>> DiscoverConsoles()
        {
            return Device.DiscoverAsync();
        }

        public Task<SmartGlass.Device> FindConsole(IPAddress addr)
        {
            return Device.PingAsync(addr.ToString());
        }
    }
}