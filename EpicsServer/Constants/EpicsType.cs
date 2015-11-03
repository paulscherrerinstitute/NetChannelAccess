using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CaSharpServer.Constants
{
    /// <summary>
    /// This enum represents the different CA value types and their corresponding intval
    /// </summary>
    public enum EpicsType : ushort
    {
        /// <summary>
        /// Plain string
        /// </summary>
        String = 0,

        /// <summary>
        /// Plain 16bit integer
        /// </summary>
        Short = 1,

        /// <summary>
        /// Plain 32bit floating point
        /// </summary>
        Float = 2,

        /// <summary>
        /// Plain enumeration (using 16bit unsigned integer)
        /// </summary>
        Enum = 3,

        /// <summary>
        /// Plain unsigned byte
        /// </summary>
        Byte = 4,

        /// <summary>
        /// Plain 32bit integer
        /// </summary>
        Int = 5,

        /// <summary>
        /// Plain 64bit floating point
        /// </summary>
        Double = 6,

        /// <summary>
        /// Extends plain string by status and severity
        /// </summary>
        Status_String = 7,

        /// <summary>
        /// Extends plain 16bit integer by status and severity
        /// </summary>
        Status_Short = 8,

        /// <summary>
        /// Extends plain 32bit floating point by status and severity
        /// </summary>
        Status_Float = 9,

        /// <summary>
        /// Extends plain enumeration by status and severity
        /// </summary>
        Status_Enum = 10,

        /// <summary>
        /// Extends plain unsigned byte by status and severity
        /// </summary>
        Status_Byte = 11,

        /// <summary>
        /// Extends plain 32bit integer by status and severity
        /// </summary>
        Status_Int = 12,

        /// <summary>
        /// Extends plain 64bit floating point by status and severity
        /// </summary>
        Status_Double = 13,

        /// <summary>
        /// Extends Status_String by timestamp
        /// </summary>
        Time_String = 14,

        /// <summary>
        /// Extends Status_Short by timestamp
        /// </summary>
        Time_Short = 15,

        /// <summary>
        /// Extends Status_Float by timestamp
        /// </summary>
        Time_Float = 16,

        /// <summary>
        /// Extends Status_Enum by timestamp
        /// </summary>
        Time_Enum = 17,

        /// <summary>
        /// Extends Status_Byte by timestamp
        /// </summary>
        Time_Byte = 18,

        /// <summary>
        /// Extends Status_Int by timestamp
        /// </summary>
        Time_Int = 19,

        /// <summary>
        /// Extends Status_Double by timestamp
        /// </summary>
        Time_Double = 20,

        /// <summary>
        /// Extends Status_String by display bounds (not used since
        /// a string cannot have display bounds)
        /// </summary>
        Display_String = 21,

        /// <summary>
        /// Extends Status_Short by display bounds
        /// </summary>
        Display_Short = 22,

        /// <summary>
        /// Extends Status_Float by display bounds
        /// </summary>
        Display_Float = 23,

        /// <summary>
        /// Extends Status_Enum by a list of enumeration labels
        /// </summary>
        Labeled_Enum = 24,

        /// <summary>
        /// Extends Status_Byte by display bounds
        /// </summary>
        Display_Byte = 25,

        /// <summary>
        /// Extends Status_Int by display bounds
        /// </summary>
        Display_Int = 26,

        /// <summary>
        /// Extends Status_Double by display bounds
        /// </summary>
        Display_Double = 27,

        /// <summary>
        /// Extends Display_String by control bounds (not used since
        /// a string cannot have control bounds)
        /// </summary>
        Control_String = 28,

        /// <summary>
        /// Extends Display_Short by control bounds
        /// </summary>
        Control_Short = 29,

        /// <summary>
        /// Extends Display_Float by control bounds
        /// </summary>
        Control_Float = 30,

        /// <summary>
        /// Not used since parent type is Labeled_Enum
        /// </summary>
        Control_Enum = 31,

        /// <summary>
        /// Extends Display_Byte by control bounds
        /// </summary>
        Control_Byte = 32,

        /// <summary>
        /// Extends Display_Int by control bounds
        /// </summary>
        Control_Int = 33,

        /// <summary>
        /// Extends Display_Double by control bounds
        /// </summary>
        Control_Double = 34,

        /// <summary>
        /// UInt32 not used by client, needed for the header fields! This Type is not valid as recordtype
        /// </summary>
        UInt = 0xFFFD,
        /// <summary>
        /// UInt16 not used by client, needed for the header fields! This Type is not valid as recordtype
        /// </summary>
        UShort = 0xFFFE,
        /// <summary>
        /// Defines an invalid data type
        /// </summary>
        Invalid = 0xFFFF
    }
}
