using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList
{
    public class CoreNetworkServerListDatabase : ICoreNetworkServerList
    {
        private List<CoreNetworkServer> servers;

        private DeltaConnection conn;
        private string enviornment;

        public CoreNetworkServerListDatabase()
        {
            this.servers = new List<CoreNetworkServer>();
        }

        public async Task Init(DeltaConnection conn, string enviornment = "prod")
        {
            //Set
            this.conn = conn;
            this.enviornment = enviornment;
            
            //Fetch servers
            var serverData = await (await conn.system_delta_servers.FindAsync(Builders<DbSystemServer>.Filter.Eq("enviornment", enviornment))).ToListAsync();
            lock(servers)
            {
                servers.Clear();
                foreach (var s in serverData)
                    AddServer(s);
            }
        }
        
        private void AddServer(DbSystemServer server)
        {
            //Convert server
            CoreNetworkServer s = new CoreNetworkServer
            {
                id = (ushort)server.server_id,
                token = ulong.Parse(server.server_token),
                address = IPAddress.Parse(server.address),
                port = server.port,
                type = Enum.Parse<CoreNetworkServerType>(server.server_type),
                manager_server_id = (ushort)server.manager_id
            };
            servers.Add(s);
        }
        
        public override CoreNetworkServer GetServerById(ushort id)
        {
            foreach (var s in servers)
            {
                if (s.id == id)
                    return s;
            }
            return null;
        }

        public override List<CoreNetworkServer> FindAllServersOfType(CoreNetworkServerType type)
        {
            List<CoreNetworkServer> servers = new List<CoreNetworkServer>();
            foreach (var s in this.servers)
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

        public override void RefreshRequested()
        {
            Init(conn, enviornment).GetAwaiter().GetResult();
        }
    }
}
