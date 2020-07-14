using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    /// <summary>
    /// The opcode. NEGATIVE VALUES ARE SYSTEM RESERVED
    /// </summary>
    public enum CoreNetworkOpcode
    {
        MESSAGE_ACK = -1,
        MESSAGE_PING = -2,
        MESSAGE_STATS = -3,

        RPC_EVENT = 1,
        RPC_REFRESH_GROUPS = 2,
        REQUEST_HEALTH_REPORT = 3,
        REMOTE_LOG = 4,
    }
}
