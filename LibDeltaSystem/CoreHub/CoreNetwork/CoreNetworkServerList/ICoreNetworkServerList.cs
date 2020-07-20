using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList
{
    public abstract class ICoreNetworkServerList
    {
        public abstract CoreNetworkServer GetServerById(ushort id);

        public abstract List<CoreNetworkServer> FindAllServersOfType(CoreNetworkServerType type);

        public abstract List<CoreNetworkServer> GetAllServers();
        public abstract void RefreshRequested();
    }
}
