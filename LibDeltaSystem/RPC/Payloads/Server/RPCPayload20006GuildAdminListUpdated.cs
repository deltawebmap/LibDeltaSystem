using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20006GuildAdminListUpdated : RPCPayload
    {
        public List<ObjectId> admins;
    }
}
