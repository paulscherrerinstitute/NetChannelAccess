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
using EpicsSharp.ChannelAccess.Constants;

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{
    /// <summary>
    /// Generic CARecord allows to store a type in the VAL property
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class CARecord<TType> : CARecord
    {
        /// <summary>
        /// Stores the actual value of the record
        /// </summary>
        TType currentValue;

        /// <summary>
        /// Access the value linked to the record
        /// </summary>
        [CAField("VAL")]
        public TType Value
        {
            get
            {
                return currentValue;
            }
            set
            {
                if ((currentValue == null && value != null) || !currentValue.Equals(value))
                    this.IsDirty = true;
                currentValue = value;
                if (Scan == ScanAlgorithm.ON_CHANGE && this.IsDirty)
                    ProcessRecord();
            }
        }
    }
}
