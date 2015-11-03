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
    /// <remarks>
    /// Command IDs are used in the CA message header for indicating the
    /// type of CA message. This determines the meaning of the other header
    /// fields.
    /// </remarks>
    internal enum CommandID : ushort
    {
        /// <summary>
        /// CA protocol version
        /// </summary>
        CA_PROTO_VERSION = 0x00,

        /// <summary>
        /// Register monitor
        /// </summary>
        CA_PROTO_EVENT_ADD = 0x01,

        /// <summary>
        /// Unregister monitor
        /// </summary>
        CA_PROTO_EVENT_CANCEL = 0x02,

        /// <summary>
        /// Read channel value (without notification)
        /// </summary>
        CA_PROTO_READ = 0x03,

        /// <summary>
        /// Write channel value (without notification)
        /// </summary>
        CA_PROTO_WRITE = 0x04,

        /// <summary>
        /// Search for a channel
        /// </summary>
        CA_PROTO_SEARCH = 0x06,

        /// <summary>
        /// Disable monitor events
        /// </summary>
        CA_PROTO_EVENTS_OFF = 0x08,

        /// <summary>
        /// Enable monitor events
        /// </summary>
        CA_PROTO_EVENTS_ON = 0x09,

        /// <summary>
        /// Error during operation
        /// </summary>
        CA_PROTO_ERROR = 0x0B,

        /// <summary>
        /// Release channel resources
        /// </summary>
        CA_PROTO_CLEAR_CHANNEL = 0x0C,

        /// <summary>
        /// Server beacon
        /// </summary>
        CA_PROTO_RSRV_IS_UP = 0x0D,

        /// <summary>
        /// Channel not found
        /// </summary>
        CA_PROTO_NOT_FOUND = 0x0E,

        /// <summary>
        /// Read channel value (with notification)
        /// </summary>
        CA_PROTO_READ_NOTIFY = 0x0F,

        /// <summary>
        /// Repeater registration confirmation
        /// </summary>
        CA_PROTO_REPEATER_CONFIRM = 0x11,

        /// <summary>
        /// Create channel
        /// </summary>
        CA_PROTO_CREATE_CHAN = 0x12,

        /// <summary>
        /// Write channel value (with notification)
        /// </summary>
        CA_PROTO_WRITE_NOTIFY = 0x13,

        /// <summary>
        /// Client user name
        /// </summary>
        CA_PROTO_CLIENT_NAME = 0x14,

        /// <summary>
        /// Client host name
        /// </summary>
        CA_PROTO_HOST_NAME = 0x15,

        /// <summary>
        /// Channel access rights
        /// </summary>
        CA_PROTO_ACCESS_RIGHTS = 0x16,

        /// <summary>
        /// Ping CA server
        /// </summary>
        CA_PROTO_ECHO = 0x17,

        /// <summary>
        /// Register client on repeater
        /// </summary>
        CA_PROTO_REPEATER_REGISTER = 0x18,

        /// <summary>
        /// Channel creation failed
        /// </summary>
        CA_PROTO_CREATE_CH_FAIL = 0x1A,

        /// <summary>
        /// Server is going down
        /// </summary>
        CA_PROTO_SERVER_DISCONN = 0x1B,

        /// <summary>
        /// Invalid response
        /// </summary>
        CA_PROTO_BAD_RESPONSE = 0xFFFF     // unofficial
    }
}
