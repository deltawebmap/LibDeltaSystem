using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub
{
    public class HealthStatusWriter
    {
        private List<byte[]> buffers = new List<byte[]>();
        private int length;
        
        public void WriteString(string key, string value)
        {
            _WriteData(key, 0x00, Encoding.UTF8.GetBytes(value));
        }

        public void WriteInt(string key, int value)
        {
            _WriteData(key, 0x01, BitConverter.GetBytes(value));
        }

        private void _WriteData(string key, byte type, byte[] data)
        {
            //Validate
            if (key.Length > byte.MaxValue || data.Length > byte.MaxValue)
                throw new Exception("Length of health status data is too long!");
            
            //Create buffer
            byte[] buffer = new byte[1 + key.Length + 2 + data.Length];

            //Write key
            buffer[0] = (byte)key.Length;
            Encoding.ASCII.GetBytes(key).CopyTo(buffer, 1);

            //Write type and length
            buffer[1 + key.Length] = type;
            buffer[2 + key.Length] = (byte)data.Length;

            //Write data
            data.CopyTo(buffer, 3 + key.Length);

            //Add
            buffers.Add(buffer);
            length += buffer.Length;
        }

        public byte[] ToBytes()
        {
            //Create buffer
            byte[] buffer = new byte[length + 1];
            int offset = 1;
            buffer[0] = (byte)buffers.Count;

            //Begin copying into buffer
            foreach(var b in buffers)
            {
                b.CopyTo(buffer, offset);
                offset += b.Length;
            }

            return buffer;
        }
    }
}
