# How to use the library

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