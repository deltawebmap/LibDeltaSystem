using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    public enum RPCOpcode
    {
        ARK_RPC_CALLBACK,
        SERVER_ACCESS_CHANGED,
        SERVER_JOINED,
        SERVER_UPDATED,
        SERVER_DELETED
    }
}
