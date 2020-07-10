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
    }
}
