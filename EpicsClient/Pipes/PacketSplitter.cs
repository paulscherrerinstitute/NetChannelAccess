using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2.Pipes
{
    /// <summary>
    /// Cuts TCP or UDP packet is EPICS messages
    /// </summary>
    class PacketSplitter : DataFilter
    {
        DataPacket remainingPacket = null;

        public override void ProcessData(DataPacket packet)
        {
            while (packet.Data.Length != 0)
            {
                // We had some left over, join with the current packet
                if (remainingPacket != null)
                {
                    packet = DataPacket.Create(remainingPacket, packet);
                    remainingPacket.Dispose();
                    remainingPacket = null;
                }

                // We don't even have a complete header, stop
                if (!packet.HasCompleteHeader)
                {
                    remainingPacket = packet;
                    return;
                }
                // Full packet, send it.
                if (packet.MessageSize == packet.Data.Length)
                {
                    this.SendData(packet);
                    return;
                }
                // More than one message in the packet, split and continue
                else if (packet.MessageSize < packet.Data.Length)
                {
                    DataPacket p = DataPacket.Create(packet, packet.MessageSize);
                    this.SendData(p);
                    DataPacket newPacket = packet.SkipSize(packet.MessageSize);
                    packet.Dispose();
                    packet = newPacket;
                }
                // Message bigger than packet.
                // Cannot be the case on UDP!
                else
                {
                    remainingPacket = packet;
                    return;
                }
            }
        }

        public void Reset()
        {
            remainingPacket = null;
        }
    }

}
