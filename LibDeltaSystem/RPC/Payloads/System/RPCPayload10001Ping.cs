using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.System
{
    public class RPCPayload10001Ping : RPCPayload
    {
        public DateTime time;
        public int nonce;
    }
}
