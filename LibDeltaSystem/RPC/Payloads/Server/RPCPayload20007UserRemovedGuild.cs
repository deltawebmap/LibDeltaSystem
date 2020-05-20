using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20007UserRemovedGuild : RPCPayload
    {
        public ObjectId user_id;
        public string name;
        public string steam_id;
        public string icon;
    }
}
