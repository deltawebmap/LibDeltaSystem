using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    /// <summary>
    /// Represents the definition for the server we're communicating with
    /// </summary>
    public class CoreNetworkServer
    {
        public ushort id;
        public ulong token;
        public CoreNetworkServerType type;
        public IPAddress address;
        public int port;
        public ushort manager_server_id; //The server that manages this server. May be 0 if there is none

    }
}
