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

        public bool[] permissions;
        public int closed_reason; //https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit

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

            //Check pseudo flags
            if (mapData == null)
                close = 32; //MAP_NOT_SUPPORTED

            //Set
            display_name = server.display_name;
            image_url = server.image_url;
            owner_uid = server.owner_uid;
            cluster_id = server.cluster_id;
            id = server.id;
            map_id = server.latest_server_map;
            permissions = server.GetPermissionFlagList();
            closed_reason = close;
        }

        public static async Task<NetGuildUser> GetGuild(DeltaConnection conn, DbServer server)
        {
            NetGuildUser g = new NetGuildUser();
            await g.SetServerData(conn, server);
            return g;
        }
    }
}
