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
using System.Collections.ObjectModel;
using System.Linq;

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{

    public class CASubArrayRecord<TArrayElementType> : CARecord where TArrayElementType : IComparable
    {

        private readonly CAArrayRecord<TArrayElementType> FullArrayRecord;

        public CASubArrayRecord(CAArrayRecord<TArrayElementType> fullArrayRecord)
        {
            FullArrayRecord = fullArrayRecord ?? throw new ArgumentNullException(nameof(fullArrayRecord));
            CanBeRemotlySet = false;
            CheckArrayElementType();

            FullArrayRecord.Value.Modified += (s, e) => {
                IsDirty = true; // Changes in the full array should trigger a subArray update
            };
        }


        private static void CheckArrayElementType()
        {
            var supportedArrayTypes = new List<Type> {
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(float),
                typeof(double),
            };
            var arrayElementType = typeof(TArrayElementType);
            if (!supportedArrayTypes.Contains(arrayElementType))
                throw new ArgumentException($"Unsupported array type '{arrayElementType.FullName}', expected one of '{string.Join(", ", supportedArrayTypes.Select(t => t.FullName))}'");
        }

        /// <summary>
        /// Allows a client to set the number of elements to read.
        /// </summary>
        /// <remarks>
        /// The actual number of elements that are returned may vary. The number of returned elements 
        /// can be read from the <see cref="ActualLength"/> property.
        /// </remarks>
        [CAField("NELM")]
        public int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                var newLength = Math.Min(MaxLength, value);
                if (_Length != newLength)
                {
                    _Length = newLength;
                    IsDirty = true;
                }
            }
        }
        private int _Length = 1;


        [CAField("INDX")]
        public int Index {
            get {
                return _Index;
            }
            set {
                if (value < 0 || value >= FullArrayLength)
                {
                    var newIndex = MaxLength - 1;
                    if (_Index != newIndex)
                    {
                        _Index = newIndex;
                        IsDirty = true;
                    }
                }
                else
                {
                    if(_Index != value)
                    {
                        _Index = value;
                        IsDirty = true;
                    }
                }
            }
        }
        private int _Index = 0;

        [CAField("NORD")]
        public int ActualLength {
            get {
                var maximumPossibleLength = FullArrayLength - Index;
                return Math.Min(Math.Min(maximumPossibleLength, Length), MaxLength);
            }
        }

        internal override int NumElementsInRecord => ActualLength;

        /// <summary>
        /// Allows the server logic to set the maximum length of the sub array
        /// </summary>
        /// <remarks>
        /// This value can be bigger than the actual length of the full array
        /// because the actual number of read elements is available in the <see cref="ActualLength"/> property.
        /// </remarks>
        [CAField("MALM")]
        public int MaxLength
        {
            get
            {
                return _MaxNumRequestedElements;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException($"{nameof(MaxLength)} must at least be 1", nameof(value));
                if(_MaxNumRequestedElements != value)
                {
                    _MaxNumRequestedElements = value;
                    IsDirty = true;
                }
            }
        }
        private int _MaxNumRequestedElements = 1;

        /// <summary>
        /// Returns the length of the full array
        /// </summary>
        public int FullArrayLength {
            get
            {
                return FullArrayRecord.Value.Length;
            }
        }

        [CAField("VAL")]
        public ReadOnlyCollection<TArrayElementType> Value
        {
            get
            {
                if (FullArrayRecord.Value == null || MaxLength < 1)
                    return null;
                var subArray = FullArrayRecord.Value
                    .AsEnumerable()
                    .Skip(Index)
                    .Take(Length)
                    .ToArray();
                return Array.AsReadOnly(subArray);
            }
        }

        [CAField("PREC")]
        public short DisplayPrecision { get; set; }

        [CAField("HOPR")]
        public float HighOperatingRange { get; set; }

        [CAField("LOPR")]
        public float LowOperatingRange { get; set; }

        [CAField("EGU")]
        public string EngineeringUnits
        {
            get
            {
                return _EngineeringUnits;
            }
            set
            {
                if (value.Length > 8)
                    throw new Exception("Cannot have more than 8 characters for the engineering unit.");
                _EngineeringUnits = value;
            }
        }
        private string _EngineeringUnits = "";
    }
}
