using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO
{
    public class RouterPacket
    {
        public short flags;
        public short packet_payload_length;
        public int total_message_length;
        public short chunk_index;
        public short opcode;
        public short sender_addr_local;
        public short sender_addr_router;
        public byte lib_version_major;
        public byte lib_version_minor;
        public byte app_version_major;
        public byte app_version_minor;
        public int message_id;
        public int response_token;
        public byte[] payload;

        public const int HEADER_SIN = 1413828164; //DNET in ASCII
        public const int HEADER_LEN = 32;

        public RouterPacket()
        {

        }

        public bool CheckFlag(int index)
        {
            return ((flags >> index) & 1) == 1;
        }

        public ulong GetGlobalMessageID()
        {
            ulong id = (ulong)message_id;
            id |= (ulong)sender_addr_local << 32;
            id |= (ulong)sender_addr_router << 48;
            return id;
        }

        public int GetLength()
        {
            return payload.Length + HEADER_LEN;
        }

        public int Deserialize(byte[] data)
        {
            if (BitConverter.ToInt32(data, 0) != HEADER_SIN)
                throw new Exception("Malformed packet! Missing header.");
            flags = BitConverter.ToInt16(data, 4);
            packet_payload_length = BitConverter.ToInt16(data, 6);
            total_message_length = BitConverter.ToInt32(data, 8);
            chunk_index = BitConverter.ToInt16(data, 12);
            opcode = BitConverter.ToInt16(data, 14);
            sender_addr_local = BitConverter.ToInt16(data, 16);
            sender_addr_router = BitConverter.ToInt16(data, 18);
            lib_version_major = data[20];
            lib_version_minor = data[21];
            app_version_major = data[22];
            app_version_minor = data[23];
            message_id = BitConverter.ToInt32(data, 24);
            response_token = BitConverter.ToInt32(data, 28);
            payload = new byte[packet_payload_length];
            Array.Copy(data, HEADER_LEN, payload, 0, payload.Length);
            return HEADER_LEN + packet_payload_length;
        }

        public void Serialize(byte[] buffer, int offset)
        {
            BitConverter.GetBytes(HEADER_SIN).CopyTo(buffer, offset + 0);
            BitConverter.GetBytes(flags).CopyTo(buffer, offset + 4);
            BitConverter.GetBytes(packet_payload_length).CopyTo(buffer, offset + 6);
            BitConverter.GetBytes(total_message_length).CopyTo(buffer, offset + 8);
            BitConverter.GetBytes(chunk_index).CopyTo(buffer, offset + 12);
            BitConverter.GetBytes(opcode).CopyTo(buffer, offset + 14);
            BitConverter.GetBytes(sender_addr_local).CopyTo(buffer, offset + 16);
            BitConverter.GetBytes(sender_addr_router).CopyTo(buffer, offset + 18);
            buffer[offset + 20] = lib_version_major;
            buffer[offset + 21] = lib_version_minor;
            buffer[offset + 22] = app_version_major;
            buffer[offset + 23] = app_version_minor;
            BitConverter.GetBytes(message_id).CopyTo(buffer, offset + 24);
            BitConverter.GetBytes(response_token).CopyTo(buffer, offset + 28);
            Array.Copy(payload, 0, buffer, offset + HEADER_LEN, payload.Length);
        }
    }
}
