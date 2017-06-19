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
using EpicsSharp.ChannelAccess.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpicsSharp.ChannelAccess.Client
{
    static class TypeHandling
    {
        public static Dictionary<Type, EpicsType> Lookup = new Dictionary<Type, EpicsType>
        {
            {typeof(uint),EpicsType.Internal_UInt},
            {typeof(ushort),EpicsType.Internal_UShort},

            {typeof(byte),EpicsType.Byte},
            {typeof(string),EpicsType.String},
            {typeof(short),EpicsType.Short},
            {typeof(int),EpicsType.Int},
            {typeof(float),EpicsType.Float},
            {typeof(double),EpicsType.Double},
            {typeof(Enum),EpicsType.Enum},

            {typeof(ExtType<byte>) ,EpicsType.Status_Byte},
            {typeof(ExtType<string>),EpicsType.Status_String},
            {typeof(ExtType<short>) ,EpicsType.Status_Short},
            {typeof(ExtType<int>)   ,EpicsType.Status_Int},
            {typeof(ExtType<float>) ,EpicsType.Status_Float},
            {typeof(ExtType<double>),EpicsType.Status_Double},
            {typeof(ExtType<Enum>)  ,EpicsType.Status_Enum},

            {typeof(ExtTimeType<byte>) ,EpicsType.Time_Byte},
            {typeof(ExtTimeType<string>),EpicsType.Time_String},
            {typeof(ExtTimeType<short>) ,EpicsType.Time_Short},
            {typeof(ExtTimeType<int>)   ,EpicsType.Time_Int},
            {typeof(ExtTimeType<float>) ,EpicsType.Time_Float},
            {typeof(ExtTimeType<double>),EpicsType.Time_Double},
            {typeof(ExtTimeType<Enum>)  ,EpicsType.Time_Enum},

            {typeof(ExtGraphic<byte>) ,EpicsType.Display_Byte},
            {typeof(ExtGraphic<string>),EpicsType.Display_String},
            {typeof(ExtGraphic<short>) ,EpicsType.Display_Short},
            {typeof(ExtGraphic<int>)   ,EpicsType.Display_Int},
            {typeof(ExtGraphic<float>) ,EpicsType.Display_Float},
            {typeof(ExtGraphic<double>),EpicsType.Display_Double},
            //{typeof(ExtGraphic<Enum>)  ,EpicsType.Display_Enum}, // Does not exists???

            {typeof(ExtControl<byte>) ,EpicsType.Control_Byte},
            {typeof(ExtControl<string>),EpicsType.Control_String},
            {typeof(ExtControl<short>) ,EpicsType.Control_Short},
            {typeof(ExtControl<int>)   ,EpicsType.Control_Int},
            {typeof(ExtControl<float>) ,EpicsType.Control_Float},
            {typeof(ExtControl<double>),EpicsType.Control_Double},
            {typeof(ExtControlEnum)    ,EpicsType.Labeled_Enum},

            // Array types

            {typeof(ExtType<byte[]>) ,EpicsType.Status_Byte},
            {typeof(ExtType<string[]>),EpicsType.Status_String},
            {typeof(ExtType<short[]>) ,EpicsType.Status_Short},
            {typeof(ExtType<int[]>)   ,EpicsType.Status_Int},
            {typeof(ExtType<float[]>) ,EpicsType.Status_Float},
            {typeof(ExtType<double[]>),EpicsType.Status_Double},
            {typeof(ExtType<Enum[]>)  ,EpicsType.Status_Enum},

            {typeof(ExtTimeType<byte[]>) ,EpicsType.Time_Byte},
            {typeof(ExtTimeType<string[]>),EpicsType.Time_String},
            {typeof(ExtTimeType<short[]>) ,EpicsType.Time_Short},
            {typeof(ExtTimeType<int[]>)   ,EpicsType.Time_Int},
            {typeof(ExtTimeType<float[]>) ,EpicsType.Time_Float},
            {typeof(ExtTimeType<double[]>),EpicsType.Time_Double},
            {typeof(ExtTimeType<Enum[]>)  ,EpicsType.Time_Enum},

            {typeof(ExtGraphic<byte[]>) ,EpicsType.Display_Byte},
            {typeof(ExtGraphic<string[]>),EpicsType.Display_String},
            {typeof(ExtGraphic<short[]>) ,EpicsType.Display_Short},
            {typeof(ExtGraphic<int[]>)   ,EpicsType.Display_Int},
            {typeof(ExtGraphic<float[]>) ,EpicsType.Display_Float},
            {typeof(ExtGraphic<double[]>),EpicsType.Display_Double},
            //{typeof(ExtGraphic<Enum>)  ,EpicsType.Display_Enum}, // Does not exists???

            {typeof(ExtControl<byte[]>) ,EpicsType.Control_Byte},
            {typeof(ExtControl<string[]>),EpicsType.Control_String},
            {typeof(ExtControl<short[]>) ,EpicsType.Control_Short},
            {typeof(ExtControl<int[]>)   ,EpicsType.Control_Int},
            {typeof(ExtControl<float[]>) ,EpicsType.Control_Float},
            {typeof(ExtControl<double[]>),EpicsType.Control_Double},
            {typeof(ExtControlEnum[])  ,EpicsType.Labeled_Enum},
        };

        public static Dictionary<EpicsType, Type> ReverseLookup;

        static TypeHandling()
        {
            ReverseLookup=new Dictionary<EpicsType, Type>();
            foreach (var i in Lookup)
                if (!ReverseLookup.ContainsKey(i.Value))
                    ReverseLookup.Add(i.Value, i.Key);
            //ReverseLookup = Lookup.ToDictionary(key => key.Value, value => value.Key);
        }

        static public int EpicsSize(object obj)
        {
            if (obj is string)
                return ((string)obj).Length;
            return EpicsSize(obj.GetType());
        }

        static public int EpicsSize(Type t)
        {
            if (t.Equals(typeof(string)))
            {
                return 40;
            }
            switch (Lookup[t])
            {
                case EpicsType.Int:
                    return 4;
                case EpicsType.Short:
                    return 2;
                case EpicsType.Byte:
                    return 1;
                case EpicsType.Float:
                    return 4;
                case EpicsType.Double:
                    return 8;
                default:
                    throw new Exception("Type not yet supported.");
            }
        }

        static public int Padding(int size)
        {
            if (size % 8 == 0)
                return 8;
            else
                return (8 - (size % 8));
        }
    }
}
