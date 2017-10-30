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
    /// <summary>
    /// Define the capabilities of a container
    /// </summary>
    /// <typeparam name="TType">The type that the container holds</typeparam>
    public abstract class Container<TType>
    {
        /// <summary>
        /// The event that gets called of elements are modified
        /// </summary>
        public abstract event EventHandler<ModificationEventArgs> Modified;

        /// <summary>
        /// The number of elements stored
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Get and set elements
        /// </summary>
        /// <param name="key">The index</param>
        /// <returns>The element</returns>
        public abstract TType this[int key] { get; set; }
    }

    /// <summary>
    /// Event arguments that get passed on with the Modified-event
    /// </summary>
    public class ModificationEventArgs : EventArgs
    {
        /// <summary>
        /// The modified index
        /// </summary>
        public int Index { get; set; }
    }
}
