/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2015  Paul Scherrer Institute, Switzerland
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using EpicsSharp.ChannelAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicsSharp.ChannelAccess.Examples
{
    /// <summary>
    /// An example Channel Access client.
    /// 
    /// This class demonstrates the client side usage of EpicsSharp,
    /// i.e. the EpicsSharp.ChannelAccess.Client.CAClient class.
    /// </summary>
    class ExampleClient : IDisposable
    {
        private CAClient client = new CAClient();

        private Dictionary<string, Channel<string>> monitoredChannels = new Dictionary<string, Channel<string>>();

        private bool startedCommunication = false;

        private string gateway = null;

        public string Gateway
        {
            get
            {
                return gateway;
            }

            set
            {
                if (startedCommunication)
                {
                    throw new NotSupportedException("You cannot change the gateway after communications started. Set the Gateway first!");
                }
                if (value == null) return;
                // This is the programmatic way to set up a Gateway for
                // PV searches. An alternative way would be to modify
                // App.config and set it there, e.g.
                //
                // <appSettings>
                //   <add key="e#ServerList" value="192.168.1.50"/>
                // </appSettings>
                client.Configuration.SearchAddress = value;
            }
        }

        static void Main(string[] args)
        {
            using (ExampleClient ec = new ExampleClient())
            {
                ec.Run();
            }
        }

        public void Dispose()
        {
            foreach (var i in monitoredChannels)
            {
                i.Value.MonitorChanged -= channel_MonitorChanged;
            }
            client.Dispose();
        }

        private void Run()
        {
            Console.WriteLine("EpicsSharp Channel Access Example Client");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Type 'help' for instructions");
            REPL();
        }

        /// <summary>
        /// "Read evaluate print" loop
        /// 
        /// A REPL is the main part of any shell like user interface.
        /// </summary>
        void REPL()
        {
            while (true)
            {
                Console.Write("ExampleClient> ");
                string input = Console.ReadLine();
                if (input == null) input = "quit"; // EOF
                string[] parts = input.Split(null);
                try
                {
                    string command = parts[0];
                    switch (command)
                    {
                        case "":
                            break;
                        case "h":
                        case "help":
                            CommandHelp();
                            break;
                        case "gw":
                        case "gateway":
                            try
                            {
                                Gateway = parts[1];
                            }
                            catch (NotSupportedException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            break;
                        case "g":
                        case "get":
                        case "caget":
                            CommandGet(parts[1]);
                            break;
                        case "m":
                        case "monitor":
                        case "camonitor":
                            CommandMonitor(parts[1]);
                            break;
                        case "q":
                        case "quit":
                        case "exit":
                            Console.WriteLine("Bye");
                            return;
                        default:
                            Console.WriteLine("Bad input. Try 'help' for instructions.");
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Bad input. Try 'help' for instructions.");
                }
            }

        }

        private void CommandMonitor(string p)
        {
            if (monitoredChannels.ContainsKey(p))
            {
                Console.WriteLine("Channel {0} is already being monitored!", p);
                return;
            }
            startedCommunication = true;
            Channel<string> channel = client.CreateChannel<string>(p);
            channel.MonitorChanged += channel_MonitorChanged;
            monitoredChannels[p] = channel;
            Console.WriteLine("Registered monitor on {0}", p);
        }

        void channel_MonitorChanged(Channel<string> sender, string newValue)
        {
            Console.WriteLine("{0}: {1}", sender.ChannelName, newValue);
        }

        private void CommandGet(string p)
        {
            startedCommunication = true;
            Channel<string> channel = client.CreateChannel<string>(p);
            string val = channel.Get();
            Console.WriteLine(val);
        }

        private void CommandHelp()
        {
            Console.WriteLine("INSTRUCTIONS");
            Console.WriteLine("------------");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  h, help                Print these instructions");
            Console.WriteLine("  q, quit                Terminate this program");
            Console.WriteLine("  g <PV>, get <PV>       Read <PV> from an IOC");
            Console.WriteLine("  gw <ADDR>, gateway <ADDR>");
            Console.WriteLine("                         Use <ADDR> as a CA gateway");
            Console.WriteLine("                         (Should be the first command.)");
            Console.WriteLine("  m <PV>, monitor <PV>   Register a monitor on <PV>");
        }
    }
}
