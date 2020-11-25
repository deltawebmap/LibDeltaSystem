using LibDeltaSystem.Entities.CommonNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadServerJoined
    {
        public string server_id;
        public NetGuildUser guild;

        public RPCPayloadServerJoined(NetGuildUser guild)
        {
            server_id = guild.id;
            this.guild = guild;
        }
    }
}
