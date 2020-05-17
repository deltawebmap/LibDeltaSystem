using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    public enum RPCOpcode
    {
        RPCSystem10001Ping = 10001,
        RPCSystem10002GuildUpdate = 10002,
        RPCServer20001ContentSync = 20001,
        RPCServer20002PartialUpdate = 20002,
        RPCServer20002CanvasEvent = 20003,

        RPCPayload30001UserServerClaimed = 30001,
        RPCPayload30001UserServerJoined = 30002
    }
}
