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

namespace EpicsSharp.ChannelAccess.Constants
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
