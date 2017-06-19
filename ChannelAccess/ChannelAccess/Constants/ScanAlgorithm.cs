/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2017  Paul Scherrer Institute, Switzerland
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
    /// The scanning algorith for a record in the CAServer.
    /// </summary>
    public enum ScanAlgorithm
    {
        /// <summary>
        /// Scan with 10Hz (10 times per second = every 100ms).
        /// </summary>
        HZ10,

        /// <summary>
        /// Scan with 5Hz (5 times per second = every 200ms).
        /// </summary>
        HZ5,

        /// <summary>
        /// Scan with 2Hz (2 times per second = every 500ms).
        /// </summary>
        HZ2,

        /// <summary>
        /// Scan every second (= 1Hz).
        /// </summary>
        SEC1,

        /// <summary>
        /// Scan every 2 seconds (= 0.5Hz).
        /// </summary>
        SEC2,

        /// <summary>
        /// Scan every 5 seconds (= 0.2Hz).
        /// </summary>
        SEC5,

        /// <summary>
        /// Scan every 10 seconds (= 0.1Hz).
        /// </summary>
        SEC10,

        ON_CHANGE,
        PASSIVE
    }
}
