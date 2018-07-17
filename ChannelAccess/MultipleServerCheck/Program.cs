using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultipleServerCheck
{
    class Program
    {
        const int CATimeout = 5000;

        static void Main(string[] args)
        {
            string channelName = "";
            string config = "255.255.255.255:5064";

            if (args.Length == 0)
            {
                Console.WriteLine("MultipleServerCheck [-e <searchConfig>] <channelName>");
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-e")
                {
                    i++;
                    config = args[i];
                }
                else
                    channelName = args[i];
            }

            CaGet(channelName, config).Wait();
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        static async Task CaGet(string channelName, string epicsConfig)
        {
            IPEndPoint remoteEP;

            // We first send a search packet
            // Using UdpClient as wraper class for our UDP communication
            using (var udpClient = new UdpClient(5432)) // listening port
            {
                var search = DataPacket.Create(channelName.Length + 8 - channelName.Length % 8);
                search.Command = 6; // CA_PROTO_SEARCH
                search.DataType = 4; // DONT_REPLY
                search.DataCount = 11; // MINOR PROTO VERSION
                search.Parameter1 = 1; // CID
                search.Parameter2 = 1; // CID
                search.SetDataAsString(channelName);

                foreach (var c in ParseAddress(epicsConfig))
                {
                    Console.WriteLine("Searching in " + c);
                    udpClient.Connect(c);
                    udpClient.Send(search.Data, search.Data.Length);

                    var sw = new Stopwatch();
                    sw.Start();
                    // No response before timeout, means no channel

                    List<string> knownServers = new List<string>();
                    bool mustStop = false;
                    while (!mustStop)
                    {
                        var receivedDataTask = udpClient.ReceiveAsync();
                        if (await Task.WhenAny(receivedDataTask, Task.Delay(CATimeout)) != receivedDataTask)
                        {
                            mustStop = true;
                            break;
                        }
                        remoteEP = receivedDataTask.Result.RemoteEndPoint;
                        if (!knownServers.Contains(remoteEP.ToString()))
                        {
                            Console.WriteLine("Response from " + remoteEP.ToString() + " after " + sw.Elapsed.ToString());
                            knownServers.Add(remoteEP.ToString());
                        }
                    }
                }
            }
        }

        static public IEnumerable<IPEndPoint> ParseAddress(string addrs)
        {
            foreach (var a in addrs.Replace(",", ";").Split(';'))
            {
                string[] parts = a.Split(':');
                IPEndPoint res;
                try
                {
                    res = new IPEndPoint(IPAddress.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()));
                }
                //catch (Exception ex)
                catch
                {
                    res = new IPEndPoint(Dns.GetHostEntry(parts[0]).AddressList.First(), int.Parse(parts[1].Trim()));
                }
                yield return res;
            }
        }
    }
}
