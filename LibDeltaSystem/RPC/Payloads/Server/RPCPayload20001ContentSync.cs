using LibDeltaSystem.RPC.Payloads.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20001ContentSync : RPCPayload
    {
        public RPCSyncType type;
        public object content;
        public int tribe_id;
        public DateTime time;
    }
}
