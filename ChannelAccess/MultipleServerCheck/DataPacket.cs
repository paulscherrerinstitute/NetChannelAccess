using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleServerCheck
{
    /// <summary>
    /// Simplified and minimalistic Data Packet.
    /// Warning: This doesn't cover all the cases, don't use it as in production.
    /// </summary>
    public class DataPacket
    {
        public byte[] Data;

        public bool ExtendedMessage => (GetUInt16(2) == 0xFFFF && GetUInt16(6) == 0x0000);

        public UInt16 Command
        {
            get
            {
                return GetUInt16(0);
            }
            set
            {
                SetUInt16(0, value);
            }
        }

        public UInt32 PayloadSize => ExtendedMessage ? GetUInt32(16) : GetUInt16(2);

        public UInt16 DataType
        {
            get
            {
                return GetUInt16(4);
            }
            set
            {
                SetUInt16(4, value);
            }
        }

        public UInt32 DataCount
        {
            get
            {
                if (ExtendedMessage)
                    return GetUInt32(20);
                return GetUInt16(6);
            }
            set
            {
                if (ExtendedMessage)
                    SetUInt32(20, value);
                else
                    SetUInt16(6, (UInt16)value);
            }
        }

        public UInt32 Parameter1
        {
            get
            {
                return GetUInt32(8);
            }
            set
            {
                SetUInt32(8, value);
            }
        }

        public UInt32 Parameter2
        {
            get
            {
                return GetUInt32(12);
            }
            set
            {
                SetUInt32(12, value);
            }
        }

        public UInt32 MessageSize => PayloadSize + HeaderSize;

        public UInt32 HeaderSize => (UInt32)(ExtendedMessage ? 24 : 16);

        public void SetDataAsString(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            Array.Clear(Data, 16, Data.Length - 16);
            Buffer.BlockCopy(b, 0, Data, 16, b.Length);
        }

        public string GetDataAsString(int offset = 0, int maxSize = 40)
        {
            // If data is smaller than what it should... return empty string
            if ((ExtendedMessage ? 24 : 16) + (int)PayloadSize > Data.Length)
                return "";
            offset += (int)this.HeaderSize;
            string ret = Encoding.Default.GetString(Data, offset, Math.Min(Data.Length - offset, maxSize));
            int indexOf = ret.IndexOf('\0');
            if (indexOf != -1)
                ret = ret.Substring(0, indexOf);
            return ret;
        }

        public UInt16 GetUInt16(int position)
        {
            return (UInt16)(((uint)Data[position] << 8) | Data[position + 1]);
        }

        public UInt32 GetUInt32(int position)
        {
            return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));
        }

        public void SetUInt16(int position, UInt16 value)
        {
            Data[position] = (byte)((value & 0xFF00u) >> 8);
            Data[position + 1] = (byte)((value) & 0xFFu);
        }

        public void SetUInt32(int position, UInt32 value)
        {
            Data[position + 0] = (byte)((value & 0xFF000000u) >> 24);
            Data[position + 1] = (byte)((value & 0x00FF0000u) >> 16);
            Data[position + 2] = (byte)((value & 0x0000FF00u) >> 8);
            Data[position + 3] = (byte)(value & 0x000000FFu);
        }

        public static DataPacket Create(int size)
        {
            DataPacket p = new DataPacket();
            p.Data = new byte[size + 16];
            p.SetUInt16(2, (ushort)(size));
            return p;
        }

        internal static DataPacket Create(byte[] buffer, int offset, int nb)
        {
            DataPacket p = new DataPacket();
            p.Data = new byte[nb];
            Array.Copy(buffer, offset, p.Data, 0, nb);
            return p;
        }

        private DataPacket()
        {
        }
    }

}
