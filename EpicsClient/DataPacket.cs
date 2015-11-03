﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;

namespace PSI.EpicsClient2
{
    /// <summary>
    /// Handles messages between workers.
    /// Can contain either a TCP/UDP packet or an EPICS message
    /// </summary>
    public class DataPacket : ICloneable, IDisposable
    {
        public byte[] Data;
        public bool NeedToFlush = false;

        public IPEndPoint Destination;
        public IPEndPoint Sender;

        /// <summary>
        /// Allows to change the sending rules
        /// </summary>
        public bool ReverseAnswer = false;

        bool? extendedMessage;
        /// <summary>
        /// Checks if it's an extended message or not.
        /// To check we look at the payload site as well as the datacount.
        /// </summary>
        public bool ExtendedMessage
        {
            get
            {
                if (!extendedMessage.HasValue)
                    extendedMessage = (GetUInt16(2) == 0xFFFF && GetUInt16(6) == 0x0000);
                return extendedMessage.Value;
            }
        }

        ushort? command;
        /// <summary>
        /// The ChannelAccess command
        /// </summary>
        public UInt16 Command
        {
            get
            {
                if (!command.HasValue)
                    command = GetUInt16(0);
                return command.Value;
            }
            set
            {
                command = value;
                SetUInt16(0, value);
            }
        }

        uint? payloadSize;
        /// <summary>
        /// Payload size either on bytes 2-4 or 16-20
        /// </summary>
        public UInt32 PayloadSize
        {
            get
            {
                if (!payloadSize.HasValue)
                {
                    payloadSize = ExtendedMessage ? GetUInt32(16) : GetUInt16(2);
                }
                return payloadSize.Value;
            }
        }

        /// <summary>
        /// Data type on bytes 4-6
        /// </summary>
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

        /// <summary>
        /// Data count either on bytes 6-8 or 20-24
        /// </summary>
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
                {
                    // Value is bigger than the limit, we should rebuild the message
                    if (value > 16000)
                    {
                        DataPacket oldPacket = (DataPacket)this.Clone();
                        Data = new byte[Data.Length + 8];
                        if (oldPacket.PayloadSize > 0)
                            Buffer.BlockCopy(oldPacket.Data, (int)oldPacket.HeaderSize, Data, 24, (int)oldPacket.PayloadSize);
                        this.Command = oldPacket.Command;
                        SetUInt32(16, oldPacket.PayloadSize); // extended payload
                        SetUInt16(2, 0xFFFF); // short payload
                        SetUInt16(6, 0x0000); // short datacount
                        this.Parameter1 = oldPacket.Parameter1;
                        this.Parameter2 = oldPacket.Parameter2;
                        SetUInt32(20, value); // extended datacount
                        DataType = oldPacket.DataType;
                    }
                    else
                        SetUInt16(6, (UInt16)value);
                }
            }
        }

        /// <summary>
        /// Parameter 1 on bytes 8-12
        /// </summary>
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

        /// <summary>
        /// Paramter 2 on bytes 12-16
        /// </summary>
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

        /// <summary>
        /// The full message size (header + payload).
        /// Can be either payload + 16 or payload + 24 in case of an extended message.
        /// </summary>
        public UInt32 MessageSize
        {
            get
            {
                return PayloadSize + HeaderSize;
            }
        }

        /// <summary>
        /// Returns the size of the header
        /// </summary>
        public UInt32 HeaderSize
        {
            get
            {
                return (UInt32)(ExtendedMessage ? 24 : 16);
            }
        }

        /// <summary>
        /// Checks (by checking the buffer size) if we have the full header or not.
        /// </summary>
        public bool HasCompleteHeader
        {
            get
            {
                if (Data.Length < 16 || ExtendedMessage && Data.Length < 24)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Retreives the payload as string.
        /// </summary>
        /// <returns></returns>
        public string GetDataAsString(int offset, int maxSize = 40)
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

        public byte[] GetPayload()
        {
            byte[] res = new byte[PayloadSize];
            Buffer.BlockCopy(Data, (int)HeaderSize, res, 0, res.Length);
            return res;
        }

        public void SetDataAsString(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            Array.Clear(Data, 16, Data.Length - 16);
            Buffer.BlockCopy(b, 0, Data, 16, b.Length);
        }

        private DataPacket()
        {
        }

        /// <summary>
        /// Returns an UInt16 at a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public UInt16 GetUInt16(int position)
        {
            return (UInt16)(((uint)Data[position] << 8) | Data[position + 1]);

            /*byte[] ushortBytes = new byte[2];
            Buffer.BlockCopy(Data, position, ushortBytes, 0, 2);

            Array.Reverse(ushortBytes);

            return BitConverter.ToUInt16(ushortBytes, 0);*/
        }

        /// <summary>
        /// Returns an UInt32 at a given position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public UInt32 GetUInt32(int position)
        {
            return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));

            /*byte[] uintBytes = new byte[4];
            Buffer.BlockCopy(Data, position, uintBytes, 0, 4);
            Array.Reverse(uintBytes);
            return BitConverter.ToUInt32(uintBytes, 0);*/
        }

        public void SetInt32(int position, int value)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            Array.Reverse(uintBytes);
            Buffer.BlockCopy(uintBytes, 0, Data, position, 4);
        }

        public Int32 GetInt32(int position)
        {
            //return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));

            byte[] uintBytes = new byte[4];
            Buffer.BlockCopy(Data, position, uintBytes, 0, 4);
            Array.Reverse(uintBytes);
            return BitConverter.ToInt32(uintBytes, 0);
        }

        public void SetInt16(int position, short value)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            Array.Reverse(uintBytes);
            Buffer.BlockCopy(uintBytes, 0, Data, position, 2);
        }

        public short GetInt16(int position)
        {
            //return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));

            byte[] uintBytes = new byte[2];
            Buffer.BlockCopy(Data, position, uintBytes, 0, 2);
            Array.Reverse(uintBytes);
            return BitConverter.ToInt16(uintBytes, 0);
        }

        public void SetFloat(int position, float value)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            Array.Reverse(uintBytes);
            Buffer.BlockCopy(uintBytes, 0, Data, position, 4);
        }

        public float GetFloat(int position)
        {
            //return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));

            byte[] uintBytes = new byte[4];
            Buffer.BlockCopy(Data, position, uintBytes, 0, 4);
            Array.Reverse(uintBytes);
            return BitConverter.ToSingle(uintBytes, 0);
        }

        public byte GetByte(int position)
        {
            byte[] bytes = new byte[1];
            Buffer.BlockCopy(Data, position, bytes, 0, 1);
            return bytes[0];
        }

        public void SetDouble(int position, double value)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            Array.Reverse(uintBytes);
            Buffer.BlockCopy(uintBytes, 0, Data, position, 8);
        }

        public double GetDouble(int position)
        {
            //return (UInt32)(((uint)Data[position + 0] << 24) | ((uint)Data[position + 1] << 16) | ((uint)Data[position + 2] << 8) | ((uint)Data[position + 3]));

            byte[] uintBytes = new byte[8];
            Buffer.BlockCopy(Data, position, uintBytes, 0, 8);
            Array.Reverse(uintBytes);
            return BitConverter.ToDouble(uintBytes, 0);
        }

        /// <summary>
        /// Writes an UInt16 at a given position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void SetUInt16(int position, UInt16 value)
        {
            Data[position] = (byte)((value & 0xFF00u) >> 8);
            Data[position + 1] = (byte)((value) & 0xFFu);
            /*byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, Data, position, bytes.Length);*/
        }

        public void SetBytes(int position, byte[] buff)
        {
            Buffer.BlockCopy(buff, 0, Data, position, buff.Length);
        }

        public void SetByte(int position, byte value)
        {
            byte[] bytes = new byte[] { value };
            Buffer.BlockCopy(bytes, 0, Data, position, 1);
        }

        /// <summary>
        /// Writes an UInt32 at a given position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        public void SetUInt32(int position, UInt32 value)
        {
            Data[position + 0] = (byte)((value & 0xFF000000u) >> 24);
            Data[position + 1] = (byte)((value & 0x00FF0000u) >> 16);
            Data[position + 2] = (byte)((value & 0x0000FF00u) >> 8);
            Data[position + 3] = (byte)(value & 0x000000FFu);
            /*byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, Data, position, bytes.Length);*/
        }

        /// <summary>
        /// Skips a given size from the data block
        /// </summary>
        /// <param name="size"></param>
        public DataPacket SkipSize(UInt32 size)
        {
            DataPacket p = DataPacket.Create(Data.Length - (int)size);
            p.Sender = this.Sender;
            p.Destination = this.Destination;
            Buffer.BlockCopy(this.Data, (int)size, p.Data, 0, p.Data.Length);
            return p;
        }

        /// <summary>
        /// Clone this packet, creating an exact copy.
        /// As the clone function is an implementation of IClonable it must return an object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            DataPacket p = DataPacket.Create(Data);
            p.Sender = this.Sender;
            p.Destination = this.Destination;
            return p;
        }

        static Dictionary<int, Stack<DataPacket>> storedPackets = new Dictionary<int, Stack<DataPacket>>();
        static SemaphoreSlim poolLocker = new SemaphoreSlim(1, 1);

        public void Dispose()
        {
        }

        public static DataPacket Create(int size)
        {
            DataPacket p = new DataPacket();
            p.Data = new byte[size];
            p.SetUInt16(2, (ushort)(size - 16));
            return p;
        }

        /// <summary>
        /// Creates a new message based on the byte buffer however use only the first "size" byte for it.
        /// </summary>
        /// <param name="buff"></param>
        public static DataPacket Create(byte[] buff)
        {
            DataPacket p = Create(buff.Length);
            Buffer.BlockCopy(buff, 0, p.Data, 0, buff.Length);
            return p;
        }

        /// <summary>
        /// Creates a new message based on the byte buffer however use only the first "size" byte for it.
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="size"></param>
        public static DataPacket Create(byte[] buff, int size)
        {
            DataPacket p = Create(size);
            Buffer.BlockCopy(buff, 0, p.Data, 0, size);
            return p;
        }

        /// <summary>
        /// Creates a new message based on an existing packed and use the "size" to extract only the first part.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="size"></param>
        public static DataPacket Create(DataPacket packet, UInt32 size)
        {
            DataPacket p = Create((int)size);
            p.Sender = packet.Sender;
            p.Destination = packet.Destination;
            Buffer.BlockCopy(packet.Data, 0, p.Data, 0, (int)size);
            return p;
        }

        /// <summary>
        /// Merges 2 packets together
        /// </summary>
        /// <param name="remaining"></param>
        /// <param name="newPacket"></param>
        public static DataPacket Create(DataPacket remaining, DataPacket newPacket)
        {
            DataPacket p = Create(remaining.Data.Length + newPacket.Data.Length);
            p.Sender = remaining.Sender;
            p.Destination = remaining.Destination;
            remaining.Data.CopyTo(p.Data, 0);
            newPacket.Data.CopyTo(p.Data, remaining.Data.Length);
            return p;
        }
    }
}
