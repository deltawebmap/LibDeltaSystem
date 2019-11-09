using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    public class RPCMessageContainer
    {
        public RPCOpcode opcode; //Message type. Never null
        public string target_server; //Target server. Could be null
        public RPCPayload payload; //Actual data
        public string source; //Where the message came from, constant
    }
}
