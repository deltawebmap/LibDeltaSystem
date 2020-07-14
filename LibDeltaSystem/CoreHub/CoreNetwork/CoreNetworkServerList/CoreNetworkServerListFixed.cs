using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList
{
    public class CoreNetworkServerListFixed : ICoreNetworkServerList
    {
        public List<CoreNetworkServer> servers;

        public CoreNetworkServerListFixed(List<CoreNetworkServer> servers)
        {
            this.servers = servers;
        }
        
        public override CoreNetworkServer GetServerById(ushort id)
        {
            foreach(var s in servers)
            {
                if (s.id == id)
                    return s;
            }
            return null;
        }

        public override List<CoreNetworkServer> FindAllServersOfType(CoreNetworkServerType type)
        {
            List<CoreNetworkServer> servers = new List<CoreNetworkServer>();
            foreach (var s in servers)
            {
                if (s.type == type)
                    servers.Add(s);
            }
            return servers;
        }

        public override List<CoreNetworkServer> GetAllServers()
        {
            return servers;
        }
    }
}
