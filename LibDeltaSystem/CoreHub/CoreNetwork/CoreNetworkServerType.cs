using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    /// <summary>
    /// Repesents the type of server
    /// </summary>
    public enum CoreNetworkServerType
    {
        HUB_SERVER, //A single server that serves as the "hub"
        RPC_SERVER, //A server that handles RPC events
        HANDLER_SERVER //A server that handles HTTP requests
    }
}
