using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class BinaryTool
    {
        public static void WriteInt64(byte[] buf, int pos, long data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteUInt64(byte[] buf, int pos, ulong data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteInt32(byte[] buf, int pos, int data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteUInt32(byte[] buf, int pos, uint data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteInt16(byte[] buf, int pos, short data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteUInt16(byte[] buf, int pos, ushort data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static void WriteFloat(byte[] buf, int pos, float data)
        {
            byte[] d = BitConverter.GetBytes(data);
            PrivateWriteBytes(d, buf, pos);
        }

        public static long ReadInt64(byte[] buf, int pos)
        {
            return BitConverter.ToInt64(PrivateReadBytes(buf, pos, 8));
        }

        public static ulong ReadUInt64(byte[] buf, int pos)
        {
            return BitConverter.ToUInt64(PrivateReadBytes(buf, pos, 8));
        }

        public static int ReadInt32(byte[] buf, int pos)
        {
            return BitConverter.ToInt32(PrivateReadBytes(buf, pos, 4));
        }

        public static uint ReadUInt32(byte[] buf, int pos)
        {
            return BitConverter.ToUInt32(PrivateReadBytes(buf, pos, 4));
        }

        public static short ReadInt16(byte[] buf, int pos)
        {
            return BitConverter.ToInt16(PrivateReadBytes(buf, pos, 2));
        }

        public static ushort ReadUInt16(byte[] buf, int pos)
        {
            return BitConverter.ToUInt16(PrivateReadBytes(buf, pos, 2));
        }

        /// <summary>
        /// Reads the 12 byte MongoDB ID
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static ObjectId ReadMongoID(byte[] buf, int pos)
        {
            return new ObjectId(PrivateReadBytes(buf, pos, 12));
        }

        /// <summary>
        /// Writes the 12 byte MongoDB ID
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static void WriteMongoID(byte[] buf, int pos, ObjectId id)
        {
            PrivateWriteBytes(id.ToByteArray(), buf, pos);
        }

        private static void PrivateWriteBytes(byte[] pending, byte[] output, int pos)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(pending);
            Array.Copy(pending, 0, output, pos, pending.Length);
        }

        private static byte[] PrivateReadBytes(byte[] buf, int pos, int len)
        {
            byte[] d = new byte[len];
            Array.Copy(buf, pos, d, 0, len);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(d);
            return d;
        }

        public static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for(int i = 0; i<a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Copies bytes directly from an array into a new array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] CopyFromArray(byte[] array, int offset, int length)
        {
            byte[] d = new byte[length];
            Array.Copy(array, offset, d, 0, length);
            return d;
        }
    }
}
