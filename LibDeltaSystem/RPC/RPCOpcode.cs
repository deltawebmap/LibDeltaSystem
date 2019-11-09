using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    public enum RPCOpcode
    {
        RPCSetSessionID = 0, //Sets the initial session ID, sent by the server
        DinosaurUpdateEvent = 1, //Synced dino was updated
    }
}
