# How to use the library

A nuget package is available on: [https://www.nuget.org/packages/ChannelAccess/](https://www.nuget.org/packages/ChannelAccess/)

## Client side

The following code will read a value of a channel (ARIDI-PCT:CURRENT) and print it out on the screen
```cs
using EpicsSharp.ChannelAccess.Client;
using System;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new CAClient();
            // Required if you use an EPICS Gateway, otherwise skip the line
            client.Configuration.SearchAddress = "sls-cagw:5062";
            var channel = client.CreateChannel<string>("ARIDI-PCT:CURRENT");
            // Synchronously read a channel and shows the value. If the channel doesn't exist an exception will be thrown.
            Console.WriteLine(channel.Get());        
        }
    }
}
```

The second example will instead "monitor" the value and call the callback every time the value changes:
```cs
using EpicsSharp.ChannelAccess.Client;
using System;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new CAClient();
            // Required if you use an EPICS Gateway, otherwise skip the line
            client.Configuration.SearchAddress = "sls-cagw:5062";
            var channel = client.CreateChannel<string>("ARIDI-PCT:CURRENT");
            // Will connect (if possible) to the channel and receive the value in the callback. In case of
            // disconnection the channel will try to reconnect automatically
            channel.MonitorChanged += (chan, val) =>
            {
                  Console.WriteLine(val);
            };
            Console.ReadKey();
        }
    }
}
```

## Server side
An EPICS server value can either update at regular intervals or "on demands" when a new value is set to the channel. The following example shows a regular interval at 10Hz (10x per seconds):

```cs
using EpicsSharp.ChannelAccess.Server;
using System;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new CAServer();
            var channel = server.CreateRecord<EpicsSharp.ChannelAccess.Server.RecordTypes.CADoubleRecord>("MYREC:VAL");
            channel.Scan = EpicsSharp.ChannelAccess.Constants.ScanAlgorithm.HZ10;
            channel.PrepareRecord += (sender, evt) =>
              {
                  channel.Value = DateTime.Now.ToOADate();
              };
            Console.ReadKey();
        }
    }
}
```

