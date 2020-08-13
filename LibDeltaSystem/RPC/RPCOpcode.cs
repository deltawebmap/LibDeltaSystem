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
        RPCServer20003CanvasEvent = 20003,
        RPCServer20004SecureModeToggled = 20004,
        RPCServer20005GuildPermissionsChanged = 20005,
        RPCServer20006GuildAdminListUpdated = 20006,
        RPCServer20007UserRemovedGuild = 20007,
        RPCServer20008ArkRpcAck = 20008,
        RPCServer20009LiveDinoUpdate = 20009,
        RPCServer20010GuildPublicDetailsChanged = 20010,
        RPCServer20011OnlinePlayersUpdated = 20011,

        RPCPayload30001UserServerClaimed = 30001,
        RPCPayload30002UserServerJoined = 30002,
        RPCPayload30003UserServerPermissionsChanged = 30003,
        RPCPayload30004UserServerRemoved = 30004
    }
}
