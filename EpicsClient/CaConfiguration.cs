using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PSI.EpicsClient2
{
    public class CaConfiguration
    {
        internal CaConfiguration()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["e#ServerList"] != null)
                SearchAddress = System.Configuration.ConfigurationManager.AppSettings["e#ServerList"];

            if (System.Diagnostics.Debugger.IsAttached)
                WaitTimeout = -1;
            else
                WaitTimeout = 5000;

            Hostname = Environment.MachineName;
            Username = Environment.UserName;
            MaxSearchSeconds = 0;
        }

        /// <summary>
        /// Defines a timeout before the search of the channels ends.
        /// Default 0 => will search all the time
        /// </summary>
        public int MaxSearchSeconds { get; set; }

        /// <summary>
        /// Stores the time each operation took.
        /// </summary>
        public bool DebugTiming = false;

        internal IPEndPoint[] SearchAddresses = new IPEndPoint[] { new IPEndPoint(IPAddress.Broadcast, 5064) };

        public string Hostname { get; internal set; }

        public string Username { get; internal set; }

        /// <summary>
        /// The timeout in miliseconds used for blocking opperations.
        /// -1 == infinite
        /// </summary>
        public int WaitTimeout { get; set; }

        /// <summary>
        /// The list of addresses used to search the channels.
        /// Addresses must be separated by semi-columns (;) , and IP / ports must be separated by columns (:)
        /// </summary>
        public string SearchAddress
        {
            get
            {
                string res = "";
                for (int i = 0; i < SearchAddresses.Length; i++)
                {
                    if (i != 0)
                        res += ";";
                    res += SearchAddresses[i].Address + ":" + SearchAddresses[i].Port;
                }
                return res;
            }
            set
            {
                value = value.Replace(' ', ';');
                value = value.Replace(',', ';');
                string[] parts = value.Split(';');
                List<IPEndPoint> ips = new List<IPEndPoint>();
                foreach (string i in parts)
                {
                    string[] p = i.Split(':');
                    IPAddress ip;
                    try
                    {
                        ip = IPAddress.Parse(p[0]);
                    }
                    catch
                    {
                        ip = Dns.GetHostEntry(p[0]).AddressList.First();
                    }
                    ips.Add(p.Length == 1 ? new IPEndPoint(ip, 5064) : new IPEndPoint(ip, int.Parse(p[1])));
                }
                SearchAddresses = ips.ToArray();
            }
        }
    }
}
