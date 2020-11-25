using LibDeltaSystem.RPC;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace LibDeltaSystem.Tools
{
    /// <summary>
    /// Managed RPC message builder tool
    /// </summary>
    public static class RPCMessageTool
    {
        public static void SendRPCMsgToUserID(DeltaConnection conn, RPCOpcode opcode, object payload, ObjectId user_id)
        {
            //Calculate
            string data = JsonConvert.SerializeObject(payload);
            int dataLen = Encoding.UTF8.GetByteCount(data);

            //Get buffer
            byte[] buffer = _CreateRPCMessageTemplate(opcode, FILTERCODE_USER, 12, (uint)dataLen, out int filterOffset, out int payloadOffset);

            //Write filter
            BinaryTool.WriteMongoID(buffer, filterOffset + 0, user_id);

            //Write payload
            Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, payloadOffset);

            //Send
            conn.net.SendRPCCommand(buffer);
        }

        public static void SendRPCMsgToUserID(DeltaConnection conn, RPCOpcode opcode, object payload, ObjectId user_id, ObjectId target_server)
        {
            //Calculate
            string data = JsonConvert.SerializeObject(payload);
            int dataLen = Encoding.UTF8.GetByteCount(data);

            //Get buffer
            byte[] buffer = _CreateRPCMessageTemplate(opcode, FILTERCODE_USER_SERVER, 16, (uint)dataLen, out int filterOffset, out int payloadOffset);

            //Write filter
            BinaryTool.WriteMongoID(buffer, filterOffset + 0, user_id);
            BinaryTool.WriteMongoID(buffer, filterOffset + 12, target_server);

            //Write payload
            Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, payloadOffset);

            //Send
            conn.net.SendRPCCommand(buffer);
        }

        public static void SendRPCMsgToServer(DeltaConnection conn, RPCOpcode opcode, object payload, ObjectId server_id)
        {
            //Calculate
            string data = JsonConvert.SerializeObject(payload);
            int dataLen = Encoding.UTF8.GetByteCount(data);

            //Get buffer
            byte[] buffer = _CreateRPCMessageTemplate(opcode, FILTERCODE_SERVER, 12, (uint)dataLen, out int filterOffset, out int payloadOffset);

            //Write filter
            BinaryTool.WriteMongoID(buffer, filterOffset, server_id);

            //Write payload
            Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, payloadOffset);

            //Send
            conn.net.SendRPCCommand(buffer);
        }

        public static void SendRPCMsgToServerTribe(DeltaConnection conn, RPCOpcode opcode, object payload, ObjectId server_id, int tribe_id)
        {
            //Calculate
            string data = JsonConvert.SerializeObject(payload);
            int dataLen = Encoding.UTF8.GetByteCount(data);

            //Get buffer
            byte[] buffer = _CreateRPCMessageTemplate(opcode, FILTERCODE_SERVER_TRIBE, 16, (uint)dataLen, out int filterOffset, out int payloadOffset);

            //Write filter
            BinaryTool.WriteMongoID(buffer, filterOffset + 0, server_id);
            BinaryTool.WriteInt32(buffer, filterOffset + 12, tribe_id);

            //Write payload
            Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, payloadOffset);

            //Send
            conn.net.SendRPCCommand(buffer);
        }

        public static void SystemNotifyUserGroupReset(DeltaConnection conn, ObjectId user)
        {
            //Get buffer
            byte[] buffer = _CreateResetGroupsTemplate(FILTERCODE_USER, 12, out int filterOffset);

            //Write filter
            BinaryTool.WriteMongoID(buffer, filterOffset, user);

            //Send
            conn.net.SendRPCCommand(buffer);
        }

        public static byte[] _CreateRPCMessageTemplate(RPCOpcode op, byte filterType, ushort filterSize, uint payloadSize, out int filterOffset, out int payloadOffset)
        {
            //1     Byte    Command Opcode
            //1     Byte    Flags
            //4     Int32   RPC Opcode
            //1     Byte    Filter Type
            //1     Byte    <Reserved>
            //2     UInt16  Filter Size
            //^     <blob>  Filter
            //4     UInt32  Payload Size
            //^     <blob>  Payload
            filterOffset = 10;
            payloadOffset = 10 + filterSize + 4;

            //Generate buffer
            byte[] payload = new byte[10 + filterSize + 4 + payloadSize];
            payload[0] = TYPECODE_MESSAGE;
            payload[1] = 0x00;
            BitConverter.GetBytes((int)op).CopyTo(payload, 2);
            payload[6] = filterType;
            payload[7] = 0x00;
            BitConverter.GetBytes(filterSize).CopyTo(payload, 8);
            //User writes filter
            BitConverter.GetBytes(payloadSize).CopyTo(payload, 10 + filterSize);
            //User writes payload

            return payload;
        }

        public static byte[] _CreateResetGroupsTemplate(byte filterType, ushort filterSize, out int filterOffset)
        {
            //1     Byte    Command Opcode
            //1     Byte    Flags
            //1     Byte    Filter Type
            //1     Byte    <Reserved>
            //2     UInt16  Filter Size
            //^     <blob>  Filter
            filterOffset = 6;

            //Generate buffer
            byte[] payload = new byte[6 + filterSize];
            payload[0] = TYPECODE_GROUP_RESET;
            payload[1] = 0x00;
            payload[2] = filterType;
            payload[3] = 0x00;
            BitConverter.GetBytes(filterSize).CopyTo(payload, 4);
            //User writes filter

            return payload;
        }

        public static byte[] _CreatePrivilegedMessageServerTemplate(RPCOpcode op, ObjectId guild, int[] payloadSizes, int[] payloadTribes, out int[] payloadOffsets)
        {
            //1     Byte    Command Opcode
            //1     Byte    Flags
            //4     Int32   RPC Opcode
            //12    MongoID Guild ID
            //4     Int32   Payload Count
            // === ARRAY BEGIN ===
            //4     Int32   Tribe ID
            //4     Int32   Payload Size
            //^     <blob>  Payload data
            // === ARRAY END ===
            payloadOffsets = new int[payloadSizes.Length];

            //Calculate the total size of all payloads, plus their headers
            int totalLength = 0;
            for (int i = 0; i < payloadSizes.Length; i++)
                totalLength += 8 + payloadSizes[i];

            //Generate buffer
            byte[] payload = new byte[22 + totalLength];
            payload[0] = TYPECODE_GROUP_RESET;
            payload[1] = 0x00;
            BitConverter.GetBytes((int)op).CopyTo(payload, 2);
            BinaryTool.WriteMongoID(payload, 6, guild);
            BitConverter.GetBytes((int)payloadSizes.Length).CopyTo(payload, 18);
            int offset = 22;
            for(int i = 0; i<payloadSizes.Length; i++)
            {
                BitConverter.GetBytes(payloadTribes[i]).CopyTo(payload, offset + 0);
                BitConverter.GetBytes(payloadSizes[i]).CopyTo(payload, offset + 4);
                payloadOffsets[i] = offset + 8;
                offset += 8 + payloadSizes[i];
            }

            return payload;
        }

        public const byte TYPECODE_MESSAGE = 0x00;
        public const byte TYPECODE_GROUP_RESET = 0x01;
        public const byte TYPECODE_PRIVILEGED_MESSAGE_SERVER = 0x02;

        public const byte FILTERCODE_USER = 0x00;
        public const byte FILTERCODE_USER_SERVER = 0x01;
        public const byte FILTERCODE_SERVER = 0x02;
        public const byte FILTERCODE_SERVER_TRIBE = 0x03;
    }
}
