using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PSI.EpicsClient2;
using System.Threading;

namespace CaTiming
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

            for (int r = 0; r < nbRepeat || nbRepeat == 0; )
            {
                if (nbRepeat > 0)
                    r++;
                if (r != 0 || nbRepeat == 0)
                {
                    Console.WriteLine("-------------------------------------------");
                    Console.WriteLine("-------------------------------------------");
                    Thread.Sleep(100);
                }
                using (EpicsClient client = new EpicsClient())
                {
                    client.Configuration.WaitTimeout = 10000;

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

                    EpicsChannel<string> channel = client.CreateChannel<string>(channelName);

                    bool hasError = false;

                    if (usingMonitor)
                    {
                        Console.WriteLine("Monitor and wait for the first value back.");
                        channel.MonitorChanged += new EpicsDelegate<string>(channel_MonitorChanged);
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
                            Console.WriteLine(ex);
                        }
                    }

                    Console.WriteLine(channel.Status.ToString());
                    TimeSpan? prevTime = null;
                    foreach (var i in channel.ElapsedTimings)
                    {
                        if (!prevTime.HasValue)
                            Console.WriteLine(i.Key + ": " + i.Value.ToString());
                        else
                            Console.WriteLine(i.Key + ": " + (i.Value - prevTime).ToString());
                        prevTime = i.Value;
                    }

                    Console.WriteLine("Total: " + prevTime.ToString());
                    Console.WriteLine("Search answered by " + channel.SearchAnswerFrom);
                    Console.WriteLine("IOC: " + channel.IOC);
                    Console.WriteLine("EPICS Type: " + channel.ChannelDefinedType);
                    if (channel.Status != ChannelStatus.CONNECTED || hasError)
                    {
                        Console.Beep();
                    }
                }
            }
        }

        static void channel_MonitorChanged(EpicsChannel<string> sender, string newValue)
        {
            Console.WriteLine("Value: " + newValue);
            gotMonitorValue.Set();
        }
    }
}
