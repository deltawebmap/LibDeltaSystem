using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadLogMessage : RPCPayload
    {
        public string message;
        public string topic;
    }
}
