/*
 *  EpicsSharp - An EPICS Channel Access library for the .NET platform.
 *
 *  Copyright (C) 2013 - 2019  Paul Scherrer Institute, Switzerland
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

namespace EpicsSharp.ChannelAccess.Constants
{
    internal enum EpicsTransitionStatus : int
    {
        ECA_NORMAL = 0,
        ECA_MAXIOC = 1,
        ECA_UKNHOST = 2,
        ECA_UKNSERV = 3,
        ECA_SOCK = 4,
        ECA_CONN = 5,
        ECA_ALLOCMEM = 6,
        ECA_UKNCHAN = 7,
        ECA_UKNFIELD = 8,
        ECA_TOLARGE = 9,
        ECA_TIMEOUT = 10,
        ECA_NOSUPPORT = 11,
        ECA_STRTOBIG = 12,
        ECA_DISCONNCHID = 13,
        ECA_BADTYPE = 14,
        ECA_CHIDNOTFND = 15,
        ECA_CHIDRETRY = 16,
        ECA_INTERNAL = 17,
        ECA_DBLCLFAIL = 18,
        ECA_GETFAIL = 19,
        ECA_PUTFAIL = 20,
        ECA_ADDFAIL = 21,
        ECA_BADCOUNT = 22,
        ECA_BADSTR = 23,
        ECA_DISCONN = 24,
        ECA_DBLCHNL = 25,
        ECA_EVDISALLOW = 26,
        ECA_BUILDGET = 27,
        ECA_NEEDSFP = 28,
        ECA_OVEVFAIL = 29,
        ECA_BADMONID = 30,
        ECA_NEWADDR = 31,
        ECA_NEWCONN = 32,
        ECA_NOCACTX = 33,
        ECA_DEFUNCT = 34,
        ECA_EMPTYSTR = 35,
        ECA_NOREPEATER = 36,
        ECA_NOCHANMSG = 37,
        ECA_DLCKREST = 38,
        ECA_SERVBEHIND = 39,
        ECA_NOCAST = 40,
        ECA_BADMASK = 41,
        ECA_IODONE = 42,
        ECA_IOINPROGRESS = 43,
        ECA_BADSYNCGRP = 44,
        ECA_PUTCBINPROG = 45,
        ECA_NORDACCESS = 46,
        ECA_NOWTACCESS = 47,
        ECA_ANACHRONISM = 48,
        ECA_NOSEARCHADDR = 49,
        ECA_NOCONVERT = 50,
        ECA_BADCHID = 51,
        ECA_BADFUNCPTR = 52,
        ECA_ISATTACHED = 53,
        ECA_UNAVAILINSERV = 54,
        ECA_CHANDESTROY = 55,
        ECA_BADPRIORITY = 56,
        ECA_NOTTHREADED = 57,
        ECA_16KARRAYCLIENT = 58,
        ECA_CONNSEQTMO = 59
    }
}
