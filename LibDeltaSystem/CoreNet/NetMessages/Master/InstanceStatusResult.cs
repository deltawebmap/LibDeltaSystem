using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public enum InstanceStatusResult : byte
    {
        ONLINE, //Connected and fully operational
        NOT_CONNECTED, //Not connected to the manager
        PING_TIMED_OUT, //Claims to be connected, but did not respond to a ping
        PING_FAILED, //Failed to send ping, probably due to a network error
    }
}
