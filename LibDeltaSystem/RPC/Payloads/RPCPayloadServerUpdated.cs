using LibDeltaSystem.Entities.CommonNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    class RPCPayloadServerUpdated
    {
        public NetGuild guild;

        public RPCPayloadServerUpdated(NetGuild guild)
        {
            this.guild = guild;
        }
    }
}
