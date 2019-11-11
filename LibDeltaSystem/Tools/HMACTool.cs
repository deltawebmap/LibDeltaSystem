using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class HMACTool
    {
        public static byte[] ComputeHMAC(byte[] key, params byte[][] data)
        {
            //Allocate memory to compute the hash of this
            int size = 0;
            int pos = 0;
            for (int i = 0; i < data.Length; i++)
                size += data[i].Length;
            byte[] buffer = new byte[size];

            //Drop all contents into this buffer
            for(int i = 0; i<data.Length; i++)
            {
                Array.Copy(data[i], 0, buffer, pos, data[i].Length);
                pos += data[i].Length;
            }

            //Compute this
            HMACSHA256 hmac = new HMACSHA256(key);
            return hmac.ComputeHash(buffer);
        }

        public static byte[] ComputeHMAC(byte[] key, byte[] salt, byte[] payload, int payload_length, int payload_offset = 0)
        {
            //Create data to check
            byte[] data = new byte[key.Length + salt.Length + payload_length];
            Array.Copy(key, 0, data, 0, key.Length);
            Array.Copy(salt, 0, data, key.Length, salt.Length);
            Array.Copy(payload, payload_offset, data, key.Length + salt.Length, payload_length);
            
            //Compute this
            HMACSHA256 hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        public static bool CompareHMAC(byte[] b1, byte[] b2)
        {
            if (b1.Length < 32 || b2.Length < 32)
                return false;
            for(int i = 0; i<32; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }
    }
}
