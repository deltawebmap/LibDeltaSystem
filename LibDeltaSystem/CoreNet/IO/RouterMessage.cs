using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO
{
    public class RouterMessage
    {
        public short opcode;
        public short sender_addr_local;
        public short sender_addr_router;
        public byte lib_version_major;
        public byte lib_version_minor;
        public byte app_version_major;
        public byte app_version_minor;
        public int message_id;
        public byte[] payload;
        public bool flagIsLast;

        private BaseRouterIO io;
        private int responseToken;
        private short chunksReceived;

        public RouterMessage(BaseRouterIO io, RouterPacket p)
        {
            this.io = io;
            this.responseToken = p.response_token;
            this.opcode = p.opcode;
            this.sender_addr_local = p.sender_addr_local;
            this.sender_addr_router = p.sender_addr_router;
            this.lib_version_major = p.lib_version_major;
            this.lib_version_minor = p.lib_version_minor;
            this.app_version_major = p.app_version_major;
            this.app_version_minor = p.app_version_minor;
            this.message_id = p.message_id;
            this.opcode = p.opcode;
            this.payload = new byte[p.total_message_length];
            this.flagIsLast = 1 == ((p.flags >> 1) & 1U);
        }

        /// <summary>
        /// For messages expecting a response, this'll write that response.
        /// </summary>
        /// <param name="data"></param>
        public void Respond(byte[] data, bool isEnd)
        {
            io.SendMessageAsResponse(0x01, data, responseToken, isEnd);
        }

        public void RespondJson<T>(T data, bool isEnd)
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            Respond(payload, isEnd);
        }

        public T DeserializeAs<T>()
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(payload));
        }

        public bool WriteChunk(RouterPacket packet)
        {
            //Calculate where to place this
            int offset = BaseRouterIO.MESSAGE_PAYLOAD_SIZE * packet.chunk_index;

            //Write
            chunksReceived++;
            Array.Copy(packet.payload, 0, payload, offset, packet.payload.Length);

            //Check if we've received the entire message
            return chunksReceived == BaseRouterIO.GetChunksInPayload(packet.total_message_length);
        }

        public ulong GetGlobalMessageID()
        {
            ulong id = (ulong)message_id;
            id |= (ulong)sender_addr_local << 32;
            id |= (ulong)sender_addr_router << 48;
            return id;
        }
    }
}
