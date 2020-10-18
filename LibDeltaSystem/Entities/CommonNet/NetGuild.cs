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
        public string cluster_id;
        public string id;
        public string map_id;
        public int permission_flags;
        public int flags;
        public string permissions_template;
        public string[] mods;

        public bool secure_mode;
        public DateTime last_secure_mode_toggled;

        public void SetServerData(DbServer server)
        {
            //Set
            display_name = server.display_name;
            image_url = server.image_url;
            cluster_id = server.cluster_id;
            id = server.id;
            map_id = server.latest_server_map;
            secure_mode = server.secure_mode;
            last_secure_mode_toggled = server.last_secure_mode_toggled;
            permission_flags = server.permission_flags;
            flags = server.flags;
            permissions_template = server.permissions_template;
            mods = server.mods;
        }

        public static NetGuildUser GetGuild(DbServer server)
        {
            NetGuildUser g = new NetGuildUser();
            g.SetServerData(server);
            return g;
        }
    }
}
