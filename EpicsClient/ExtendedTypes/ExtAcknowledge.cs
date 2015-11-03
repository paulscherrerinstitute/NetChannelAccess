using System;
using System.Collections.Generic;
using System.Text;

namespace PSI.EpicsClient2
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
