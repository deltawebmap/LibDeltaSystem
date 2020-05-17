using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetGuild
    {
        public string display_name;
        public string image_url;
        public string owner_uid;
        public string cluster_id;
        public string id;
        public string map_id;

        public bool secure_mode;
        public DateTime last_secure_mode_toggled;

        public async Task SetServerData(DeltaConnection conn, DbServer server)
        {
            //Get map info
            string mapName = null;
            var mapData = await server.GetMapEntryAsync(conn);
            if (mapData != null)
                mapName = mapData.displayName;

            //Get closed reason, if any
            int close = -1;
            for (int i = 31; i >= 0; i--)
            {
                if (server.CheckLockFlag(i))
                    close = i;
            }

            //Set
            display_name = server.display_name;
            image_url = server.image_url;
            owner_uid = server.owner_uid?.ToString();
            cluster_id = server.cluster_id;
            id = server.id;
            map_id = server.latest_server_map;
            secure_mode = server.secure_mode;
            last_secure_mode_toggled = server.last_secure_mode_toggled;
        }

        public static async Task<NetGuildUser> GetGuild(DeltaConnection conn, DbServer server)
        {
            NetGuildUser g = new NetGuildUser();
            await g.SetServerData(conn, server);
            return g;
        }
    }
}
