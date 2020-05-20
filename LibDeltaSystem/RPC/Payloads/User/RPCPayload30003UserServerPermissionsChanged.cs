using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.User
{
    public class RPCPayload30003UserServerPermissionsChanged : RPCPayload
    {
        public ObjectId guild_id;
        public bool is_admin;
    }
}
