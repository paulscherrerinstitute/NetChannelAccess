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
    public class SubArrayContainer<TType> : Container<TType> where TType : IComparable
    {
        /// <summary>
        /// The parent container for the complete array data
        /// </summary>
        public ArrayContainer<TType> Data { get; private set; }

        /// <summary>
        /// Event that gets called if a part of the subArray was modified
        /// </summary>
        public override event EventHandler<ModificationEventArgs> Modified;

        /// <summary>
        /// The index to start sending elements from
        /// </summary>
        internal int INDX;

        /// <summary>
        /// Number of elements to send, starting at INDX
        /// </summary>
        internal int NELM;

        /// <summary>
        /// Create a new SubArrayContainer based on a fixed-size array
        /// </summary>
        /// <param name="size">The size of the fixed size array</param>
        public SubArrayContainer(int size)
        {
            Data = new ArrayContainer<TType>(size);
            Data.Modified += DataModified;
            INDX = 0;
            NELM = size;
        }

        /// <summary>
        /// Gets called if the data array gets modified
        /// </summary>
        /// <param name="sender">The data ArrayContainer</param>
        /// <param name="e">the event arguments</param>
        private void DataModified(object sender, ModificationEventArgs e)
        {
            if (e.Index >= INDX && e.Index < INDX + NELM && Modified != null)
                Modified(this, e);
        }

        /// <summary>
        /// Set the range for the subArray
        /// </summary>
        /// <param name="indx">The starting index</param>
        /// <param name="nelm">The number of elements</param>
        public void SetSubArray(int indx, int nelm)
        {
            if (indx < 0 || indx >= Data.Length)
                throw new ArgumentOutOfRangeException("INDX out of bounds: " + indx);
            if (nelm < 0)
                throw new ArgumentOutOfRangeException("NELM must be greater or equal to 0");
            if (indx + nelm > Data.Length)
                throw new ArgumentOutOfRangeException("INDX + NELM out of bounds: " + (indx + nelm));
            if (INDX == indx && NELM == nelm)
                return;
            INDX = indx;
            NELM = nelm;
            Modified?.Invoke(this, null);
        }

        /// <summary>
        /// The length of the subArray
        /// </summary>
        public override int Length
        {
            get
            {
                return NELM;
            }
        }

        /// <summary>
        /// Access the subArray
        /// </summary>
        /// <param name="key">The index</param>
        /// <returns>Item at specified index in subArray</returns>
        public override TType this[int key]
        {
            get
            {
                if(key >= NELM)
                    throw new ArgumentOutOfRangeException("Cannot access array members out of the subArrays bounds");
                return Data[key + INDX];
            }
            set
            {
                if (key >= NELM || key < 0)
                    throw new ArgumentOutOfRangeException("Cannot access array members out of the subArrays bounds");
                Data[key + INDX] = value;
            }
        }

        public override IEnumerator<TType> GetEnumerator()
        {
            for (var i = this.INDX; i < this.INDX + this.NELM; i++)
                yield return Data[i];
        }

        /*IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = this.INDX; i < this.INDX + this.NELM; i++)
                yield return Data[i];
        }*/
    }
}
