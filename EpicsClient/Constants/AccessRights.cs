using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSI.EpicsClient2
{
    /// <summary>
    /// Channel access rights
    /// </summary>
    [Flags]
    public enum AccessRights : uint
    {
        /// <summary>
        /// no write nor reade access 
        /// </summary>
        NoAccess = 0,
        /// <summary>
        /// it means you can only read.
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// you can only write
        /// </summary>
        WriteOnly = 2,
        /// <summary>
        /// Read and Write Rights
        /// </summary>
        ReadAndWrite = 3,
    }
}
