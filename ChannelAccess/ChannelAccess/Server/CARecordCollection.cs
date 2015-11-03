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
using System.Threading;
using EpicsSharp.ChannelAccess.Constants;
using EpicsSharp.ChannelAccess.Server.RecordTypes;

namespace EpicsSharp.ChannelAccess.Server
{
    /// <summary>
    /// Contains all the records offered by the server and handle the process loop.
    /// </summary>
    internal class CARecordCollection
    {
        Thread processThread;
        Dictionary<string, CARecord> records = new Dictionary<string, CARecord>();
        bool running = true;

        internal CARecordCollection()
        {
            processThread = new Thread(new ThreadStart(ProcessLoop));
            processThread.IsBackground = true;
            processThread.Start();
        }

        internal void Add(CARecord record)
        {
            lock (records)
            {
                records.Add(record.Name, record);
            }
        }

        internal void Remove(CARecord record)
        {
            lock (records)
            {
                records.Remove(record.Name);
            }
        }

        internal bool Contains(string name)
        {
            lock (records)
            {
                return records.ContainsKey(name);
            }
        }

        internal CARecord this[string key]
        {
            get
            {
                lock (records)
                {
                    return records[key];
                }
            }
        }

        internal void ProcessLoop()
        {
            DateTime nextLoop = DateTime.Now;
            int step = 0;
            while (running)
            {
                nextLoop = nextLoop.AddMilliseconds(100);
                lock (records)
                {
                    foreach (var i in records)
                    {
                        switch (i.Value.Scan)
                        {
                            case ScanAlgorithm.HZ10:
                                i.Value.CallPrepareRecord();
                                i.Value.ProcessRecord();
                                break;
                            case ScanAlgorithm.HZ5:
                                if (step % 2 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.HZ2:
                                if (step % 5 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.SEC1:
                                if (step % 10 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.SEC2:
                                if (step % 20 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.SEC5:
                                if (step % 50 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.SEC10:
                                if (step % 100 == 0)
                                {
                                    i.Value.CallPrepareRecord();
                                    i.Value.ProcessRecord();
                                }
                                break;
                            case ScanAlgorithm.ON_CHANGE:
                                break;
                            case ScanAlgorithm.PASSIVE:
                                break;
                            default:
                                break;
                        }
                    }
                }
                // Sleep max 100 milliseconds (and tries to adjust our loop)
                if ((nextLoop - DateTime.Now).Milliseconds > 0)
                {
                    try
                    {
                        Thread.Sleep((nextLoop - DateTime.Now).Milliseconds);
                    }
                    catch
                    {
                    }
                }
                step = (step + 1) % 100;
            }
        }

        public void Dispose()
        {
            if (running == false)
                return;
            running = false;
        }
    }
}
