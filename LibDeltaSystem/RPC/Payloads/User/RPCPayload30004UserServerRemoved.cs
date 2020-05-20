using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.User
{
    public class RPCPayload30004UserServerRemoved : RPCPayload
    {
        public ObjectId guild_id;
    }
}
