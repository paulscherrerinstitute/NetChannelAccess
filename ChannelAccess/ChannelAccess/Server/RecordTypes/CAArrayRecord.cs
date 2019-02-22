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

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{
    public abstract class CAArrayRecord : CARecord { }

    public class CAArrayRecord<TType> : CAArrayRecord
        where TType : IComparable
    {

        public CAArrayRecord(int size)
        {
            Value = new ArrayContainer<TType>(size);
            Value.Modified += (s, e) => {
                IsDirty = true;
            };
        }

        /// <summary>
        /// Access the value linked to the record
        /// </summary>
        [CAField("VAL")]
        public ArrayContainer<TType> Value { get; private set; }

        string engineeringUnits = "";
        /// <summary>
        /// Defines the value of the Engineering Units.
        /// </summary>
        [CAField("EGU")]
        public string EngineeringUnits
        {
            get
            {
                return engineeringUnits;
            }
            set
            {
                if (value.Length > 8)
                    throw new Exception("Cannot have more than 8 characters for the engineering unit.");
                engineeringUnits = value;
            }
        }

        /// <summary>
        /// Defines the Display Precision.
        /// </summary>
        [CAField("PREC")]
        public short DisplayPrecision { get; set; }

        internal override int NumElementsInRecord => Value.Length;
    }
}
