using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Reflection;
using System.Text;

namespace WebDisplay
{
    public class UpdateHub : Hub
    {
        static Dictionary<string, EpicsProxy> epicsProxies = new Dictionary<string, EpicsProxy>();

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            lock (epicsProxies)
            {
                if (epicsProxies.ContainsKey(Context.ConnectionId))
                {
                    epicsProxies[Context.ConnectionId].Dispose();
                    epicsProxies.Remove(Context.ConnectionId);
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        private EpicsProxy EpicsProxy
        {
            get
            {
                lock (epicsProxies)
                {
                    if (!epicsProxies.ContainsKey(Context.ConnectionId))
                    {
                        epicsProxies.Add(Context.ConnectionId, new EpicsProxy());
                        epicsProxies[Context.ConnectionId].ChannelEnumStates += UpdateHub_ChannelEnumStates;
                        epicsProxies[Context.ConnectionId].ChannelUpdated += UpdateHub_ChannelUpdated;
                    }
                    return epicsProxies[Context.ConnectionId];
                }
            }
        }

        void UpdateHub_ChannelEnumStates(string channel, string[] states)
        {
            this.Clients.Caller.UpdateEnumStates(channel, states);
        }

        private void UpdateHub_ChannelUpdated(string channel, string value)
        {
            this.Clients.Caller.UpdateReceived(channel, value);
        }

        public void RegisterChannel(string channelName)
        {
            EpicsProxy.Subsribe(channelName);
        }

        private string GetAvailableComponents()
        {
            var displays = Assembly.GetExecutingAssembly().GetTypes().Where(row => row.IsSubclassOf(typeof(DisplayControl))).ToList();

            StringBuilder res = new StringBuilder();

            foreach (var i in displays)
            {
                res.Append("{Name:'" + i.Name + "'");
                res.Append(",Properties:[");
                bool isFirst = true;
                foreach (var j in i.GetProperties().Where(row => row.GetCustomAttributes(typeof(DisplayPropertyAttribute), true).Any()))
                {
                    if (!isFirst)
                        res.Append(",");
                    res.Append("{Name:'" + j.Name + "'}");
                }
                res.Append("]");
                res.Append("}");
            }

            return "[" + res.ToString() + "]";
        }
    }
}