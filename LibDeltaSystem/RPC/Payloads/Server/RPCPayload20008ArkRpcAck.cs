using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20008ArkRpcAck : RPCPayload
    {
        public ObjectId rpc_id;
        public Dictionary<string, string> custom_data;
    }
}
