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
    public delegate void ReceiveDataDelegate(DataPacket packet);

    abstract class DataFilter : IDisposable
    {
        public event ReceiveDataDelegate ReceiveData;
        public DataPipe Pipe;

        public abstract void ProcessData(DataPacket packet);

        /// <summary>
        /// Sends the DataPacket further in the chain
        /// </summary>
        /// <param name="packet"></param>
        public void SendData(DataPacket packet)
        {
            if (ReceiveData != null)
                ReceiveData(packet);
        }

        public virtual void Dispose()
        {
        }
    }
}
