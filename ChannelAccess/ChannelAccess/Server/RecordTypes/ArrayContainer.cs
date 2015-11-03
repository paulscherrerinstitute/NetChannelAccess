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

namespace EpicsSharp.ChannelAccess.Server.RecordTypes
{
    public class ArrayContainer<TType>: IEnumerable<TType> where TType : IComparable
    {
        internal TType[] arrayValues;
        internal event EventHandler ArrayModified;

        public ArrayContainer(int size)
        {
            arrayValues = new TType[size];
        }

        public int Length
        {
            get
            {
                return arrayValues.Length;
            }
        }


        public TType this[int key]
        {
            get
            {
                return arrayValues[key];
            }
            set
            {
                if (arrayValues[key].CompareTo(value) != 0 && ArrayModified != null)
                    ArrayModified(this, null);
                arrayValues[key] = value;
            }
        }

        public IEnumerator<TType> GetEnumerator()
        {
            return (IEnumerator<TType>)arrayValues.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return arrayValues.GetEnumerator();
        }
    }
}
