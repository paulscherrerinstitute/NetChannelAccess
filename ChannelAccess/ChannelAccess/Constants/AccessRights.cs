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
    /// <summary>
    /// Access rights to a channel.
    /// </summary>
    public enum AccessRights : ushort
    {
        /// <summary>
        /// You can neither read from nor write to the channel.
        /// </summary>
        NoAccess = 0,

        /// <summary>
        /// You can read from, but not write to the channel.
        /// </summary>
        ReadOnly = 1,

        /// <summary>
        /// You can write to, but not read from the channel.
        /// </summary>
        WriteOnly = 2,

        /// <summary>
        /// You can both, read from and write to the channel.
        /// </summary>
        ReadAndWrite = 3
    }
}
