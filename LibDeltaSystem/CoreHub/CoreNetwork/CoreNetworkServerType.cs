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
        HANDLER_SERVER, //A server that handles HTTP requests
        MASTER_SERVER, //The server on the root of the Delta Web Map api deltamap.net/api/
        PROCESS_MANAGER, //A server that spins off sub-processes
        TEST, //A server that is merely a test
        ECHO_CONTENT,
        SYNC_INGEST_V1
    }
}
