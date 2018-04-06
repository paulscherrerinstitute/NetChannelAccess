using EpicsSharp.ChannelAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CaConnectionChecks
{
    class Program
    {
        static AutoResetEvent gotMonitorValue = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            int nbRepeat = 1;
            if (args.Any(row => row == "-r"))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-r")
                    {
                        i++;
                        nbRepeat = int.Parse(args[i]);
                    }
                }
            }

            for (int r = 0; r < nbRepeat || nbRepeat == 0;)
            {
                if (nbRepeat > 0)
                    r++;

                if (r != 0 || nbRepeat == 0)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("-------------------------------------------");
                    Thread.Sleep(100);
                }

                using (var client = new CAClient())
                {
                    client.Configuration.WaitTimeout = 10000;
                    client.Configuration.DebugTiming = true;

                    string channelName = "";
                    bool usingMonitor = false;

                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-e")
                        {
                            i++;
                            client.Configuration.SearchAddress = args[i];
                        }
                        else if (args[i] == "-m")
                            usingMonitor = true;
                        else
                            channelName = args[i];
                    }

                    client.Configuration.DebugTiming = true;
                    Console.WriteLine("EPICS Configuration: " + client.Configuration.SearchAddress);
                    Console.WriteLine("Trying to read " + channelName);
                    var sw = new Stopwatch();
                    sw.Start();

                    var channel = client.CreateChannel<string>(channelName);

                    bool hasError = false;

                    if (usingMonitor)
                    {
                        Console.WriteLine("Monitor and wait for the first value back.");
                        channel.MonitorChanged += (c, v) =>
                        {
                            Console.WriteLine("Value: " + v);
                            gotMonitorValue.Set();
                        };
                        try
                        {
                            gotMonitorValue.WaitOne(5000);
                        }
                        catch
                        {
                            hasError = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Get and waits the value back.");
                        try
                        {
                            Console.WriteLine(channel.Get());
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            //Console.WriteLine(ex);
                        }
                    }

                    sw.Stop();
                    Console.WriteLine("Status: " + channel.Status.ToString());
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("Timings:");
                    Console.WriteLine("-------------------------------------------");

                    TimeSpan? prevTime = null;
                    foreach (var i in channel.ElapsedTimings)
                    {
                        if (!prevTime.HasValue)
                            Console.WriteLine(i.Key + ": " + i.Value.ToString());
                        else
                            Console.WriteLine(i.Key + ": " + (i.Value - prevTime).ToString());
                        prevTime = i.Value;
                    }

                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("Total: " + sw.Elapsed.ToString());
                    Console.WriteLine("Search answered by " + channel.SearchAnswerFrom);
                    Console.WriteLine("IOC: " + channel.IOC);
                    Console.WriteLine("EPICS Type: " + channel.ChannelDefinedType);
                    if (channel.Status != EpicsSharp.ChannelAccess.Constants.ChannelStatus.CONNECTED || hasError)
                    {
                        Console.Beep();
                    }
                }
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
