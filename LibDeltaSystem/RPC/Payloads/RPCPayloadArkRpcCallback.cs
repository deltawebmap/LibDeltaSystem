using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    class RPCPayloadArkRpcCallback
    {
        public ObjectId rpc_id;
        public Dictionary<string, string> custom_data;

        public RPCPayloadArkRpcCallback(ObjectId rpc_id, Dictionary<string, string> custom_data)
        {
            this.rpc_id = rpc_id;
            this.custom_data = custom_data;
        }
    }
}
