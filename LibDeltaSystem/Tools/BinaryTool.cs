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
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(d);
            Array.Copy(d, 0, buf, pos, 4);
        }

        public static int ReadInt32(byte[] buf, int pos)
        {
            byte[] d = new byte[4];
            Array.Copy(buf, pos, d, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(d);
            return BitConverter.ToInt32(d);
        }
    }
}
