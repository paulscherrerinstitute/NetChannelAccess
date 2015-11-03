using PSI.EpicsClient2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebDisplay
{
    public delegate void ChannelUpdate(string channel, string value);

    public delegate void ChannelEnumUpdate(string channel, string[] states);

    public class EpicsProxy : IDisposable
    {
        static EpicsClient epicsClient;
        static Dictionary<string, EpicsChannel> knownChannels = new Dictionary<string, EpicsChannel>();
        static Dictionary<string, int> knownChannelsUsage = new Dictionary<string, int>();
        static Dictionary<string, DateTime> knownChannelsLastUsage = new Dictionary<string, DateTime>();
        static Dictionary<string, string[]> knownChannelsEnum = new Dictionary<string, string[]>();
        List<EpicsChannel> subscribedChannels = new List<EpicsChannel>();

        public event ChannelUpdate ChannelUpdated;

        public event ChannelEnumUpdate ChannelEnumStates;

        static Thread cleanup;

        static EpicsProxy()
        {
            epicsClient = new EpicsClient();
            epicsClient.Configuration.WaitTimeout = 5000;
            cleanup = new Thread(CleanOldChannels);
            cleanup.IsBackground = true;
            cleanup.Start();
        }

        static void CleanOldChannels(object obj)
        {
            while (true)
            {
                Thread.Sleep(10000);

                lock (knownChannels)
                {
                    var lastUsage = knownChannelsLastUsage.Where(row => (DateTime.Now - row.Value).TotalSeconds > 20).ToList();
                    foreach (var i in lastUsage)
                    {
                        knownChannels[i.Key].Dispose();
                        knownChannels.Remove(i.Key);
                        knownChannelsUsage.Remove(i.Key);
                        knownChannelsLastUsage.Remove(i.Key);
                    }
                }
            }
        }

        async Task ConnectAsync(string channelName)
        {
            lock (knownChannels)
            {
                if (knownChannelsLastUsage.ContainsKey(channelName))
                    knownChannelsLastUsage.Remove(channelName);
            }

            bool shouldCheck = true;
            EpicsChannel c = null;
            lock (knownChannels)
            {
                shouldCheck = !knownChannels.ContainsKey(channelName);
            }
            if (shouldCheck)
            {
                c = epicsClient.CreateChannel(channelName);
                await c.ConnectAsync();
            }
            lock (knownChannels)
            {
                if (!knownChannels.ContainsKey(channelName) && c != null)
                {
                    if (c.ChannelDefinedType != null && c.ChannelDefinedType.Name == "Enum")
                    {
                        knownChannels.Add(channelName, epicsClient.CreateChannel<int>(channelName));
                        if (!knownChannelsEnum.ContainsKey(channelName))
                        {
                            var t = epicsClient.CreateChannel<ExtControlEnum>(channelName);
                            var v = t.Get();
                            knownChannelsEnum.Add(channelName, v.States);
                        }
                    }
                    else
                    {
                        knownChannels.Add(channelName, c);
                    }
                    knownChannelsUsage.Add(channelName, 0);
                }
                if (knownChannelsEnum.ContainsKey(channelName))
                {
                    if (ChannelEnumStates != null)
                        ChannelEnumStates(channelName, knownChannelsEnum[channelName]);
                }
                knownChannels[channelName].MonitorChanged += channel_MonitorChanged;
                knownChannelsUsage[channelName]++;
                subscribedChannels.Add(knownChannels[channelName]);
            }
        }

        public void Subsribe(string channelName)
        {
            Task.Run(() => ConnectAsync(channelName));
        }

        void channel_MonitorChanged(EpicsChannel sender, object newValue)
        {
            if (ChannelUpdated != null)
                ChannelUpdated(sender.ChannelName, newValue.ToString());
        }


        public void Dispose()
        {
            lock (knownChannels)
            {
                foreach (var i in subscribedChannels)
                {
                    i.MonitorChanged -= channel_MonitorChanged;
                    knownChannelsUsage[i.ChannelName]--;
                    if (knownChannelsUsage[i.ChannelName] < 1)
                        knownChannelsLastUsage.Add(i.ChannelName, DateTime.Now);
                }
            }
        }
    }
}