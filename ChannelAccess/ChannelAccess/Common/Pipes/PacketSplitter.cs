using EpicsSharp.ChannelAccess.Common;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpicsSharp.Common.Pipes
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
