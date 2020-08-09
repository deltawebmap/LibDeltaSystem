using LibDeltaSystem.Entities.CommonNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.User
{
    public class RPCPayload30002UserServerJoined : RPCPayload
    {
        public NetGuild guild;
    }
}
