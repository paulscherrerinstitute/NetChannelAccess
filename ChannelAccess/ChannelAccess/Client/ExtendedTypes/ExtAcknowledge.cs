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
using System.Text;

namespace EpicsSharp.ChannelAccess.Client
{
    /// <summary>
    /// extended epics Acknowledge type <br/> serves severity, status, value, precision (for double and float), unittype 
    /// and a bunch of limits. 
    /// </summary>
    /// <typeparam name="TType">generic datatype for value</typeparam>
    public class ExtAcknowledge<TType> : ExtType<TType>
    {
        internal ExtAcknowledge()
        {
        }

        /// <summary>
        /// transient of the acknowledge message
        /// </summary>
        public short AcknowledgeTransient { get; internal set; }
        /// <summary>
        /// Severity of the acknowledge serverity
        /// </summary>
        public short AcknowledgeSeverity { get; internal set; }
    }
}
