using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer.Constants
{
    /// <summary>
    /// Channel access rights
    /// </summary>
    public enum AccessRights
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

        /// <summary>
        /// Read and Write Rights
        /// </summary>
        NotSet = 4
    }
}
