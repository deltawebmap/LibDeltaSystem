using LibDeltaSystem.Entities.CommonNet;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.System
{
    public class RPCPayload10002GuildUpdate : RPCPayload
    {
        public string server_id;
        public NetGuild guild;
        public DateTime time;
    }
}
