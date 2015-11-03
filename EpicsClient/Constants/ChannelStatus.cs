using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2
{
    // Summary:
    //     Current connection status of a channel
    public enum ChannelStatus
    {
        // Summary:
        //     Channel was just created and is trying to be established.
        REQUESTED = 0,
        //
        // Summary:
        //     Channel is connected to an IOC and able to work.
        CONNECTED = 1,
        //
        // Summary:
        //     Channel was connected, lost connection, and is not working now. But will
        //     try to reconnect automaticly
        DISCONNECTED = 2,
        DISPOSED=3,
    }
}
