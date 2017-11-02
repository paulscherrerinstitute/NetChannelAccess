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
using System.Collections;
using System.Collections.Generic;

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{

    public class ArrayContainer<TType> : Container<TType>, IEnumerable<TType> where TType : IComparable
    {
        /// <summary>
        /// The modification event
        /// </summary>
        public override event EventHandler<ModificationEventArgs> Modified;

        /// <summary>
        /// The values
        /// </summary>
        internal TType[] values;

        /// <summary>
        /// Create a new ArrayContainer with the given size
        /// </summary>
        /// <param name="size">The number of elements</param>
        public ArrayContainer(int size)
        {
            values = new TType[size];
        }

        /// <summary>
        /// The length of the array
        /// </summary>
        public override int Length
        {
            get
            {
                return values.Length;
            }
        }

        /// <summary>
        /// Access the array elements
        /// </summary>
        /// <param name="key">The index</param>
        /// <returns>The elemnt at the given index</returns>
        public override TType this[int key]
        {
            get
            {
                return values[key];
            }
            set
            {
                if (values[key].CompareTo(value) != 0 && Modified != null)
                    Modified(this, new ModificationEventArgs { Index = key});
                values[key] = value;
            }
        }

        public IEnumerator<TType> GetEnumerator()
        {
            foreach (var i in values)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
