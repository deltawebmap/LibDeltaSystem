using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadRPCSetSessionID : RPCPayload
    {
        public string session_id;
        public string host;
    }
}
