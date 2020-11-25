using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    class RPCPayloadServerDeleted
    {
        public ObjectId guild_id;

        public RPCPayloadServerDeleted(ObjectId guild_id)
        {
            this.guild_id = guild_id;
        }
    }
}
