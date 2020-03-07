using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class BinaryTool
    {
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

        public static int ReadInt32(byte[] buf, int pos)
        {
            return BitConverter.ToInt32(PrivateReadBytes(buf, pos, 4));
        }

        public static uint ReadUInt32(byte[] buf, int pos)
        {
            return BitConverter.ToUInt32(PrivateReadBytes(buf, pos, 4));
        }

        private static void PrivateWriteBytes(byte[] pending, byte[] output, int pos)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(pending);
            Array.Copy(pending, 0, output, pos, pending.Length);
        }

        private static byte[] PrivateReadBytes(byte[] buf, int pos, int len)
        {
            byte[] d = new byte[4];
            Array.Copy(buf, pos, d, 0, 4);
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
    }
}
