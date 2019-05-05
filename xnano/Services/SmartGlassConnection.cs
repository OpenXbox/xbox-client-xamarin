
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
    public static class SmartGlassConnection
    {
        public static GamestreamConfiguration StandardGamestreamConfig
            => GamestreamConfiguration.GetStandardConfig();

        public static async Task<GamestreamSession> ConnectViaSmartGlass(string ipAddress)
        {
            string lastStatus = String.Empty;
            try
            {
                lastStatus = "Connecting to console";
                var client = await SmartGlassClient.ConnectAsync(ipAddress);

                lastStatus = "Waiting for Broadcast channel";
                await client.BroadcastChannel.WaitForEnabledAsync(
                    TimeSpan.FromSeconds(5));

                if (!client.BroadcastChannel.Enabled)
                {
                    throw new NotSupportedException("Broadcast channel is not enabled");
                }

                lastStatus = "Starting gamestream via Broadcast channel";
                return await client.BroadcastChannel.StartGamestreamAsync(StandardGamestreamConfig);
            }
            catch (TimeoutException)
            {
                throw new Exception($"{lastStatus} timed out!");
            }
            catch (NotSupportedException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new Exception($"Unknown error: {lastStatus}, message: {e.Message}");
            }
        }

        public static async Task<NanoClient> ConnectViaNano(string ipAddress, GamestreamSession session)
        {
            string lastStatus = String.Empty;
            try
            {
                var client = new NanoClient(ipAddress, session);
                lastStatus = "Initializing Nano protocol";
                await client.InitializeProtocolAsync();

                var videoFmt = client.VideoFormats[0];
                var audioFmt = client.AudioFormats[0];

                lastStatus = "Initializing Nano stream";
                await client.InitializeStreamAsync(audioFmt, videoFmt);

                lastStatus = "Initializing input channel";
                await client.OpenInputChannelAsync(1280, 720);

                return client;
            }
            catch (TimeoutException)
            {
                throw new Exception($"{lastStatus} timed out!");
            }
            catch (Exception e)
            {
                throw new Exception($"Unknown error: {lastStatus}, message: {e.Message}");
            }
        }

        public static Task<IEnumerable<SmartGlass.Device>> DiscoverConsoles()
        {
            return Device.DiscoverAsync();
        }

        public static Task<SmartGlass.Device> FindConsole(IPAddress addr)
        {
            return Device.PingAsync(addr.ToString());
        }
    }
}