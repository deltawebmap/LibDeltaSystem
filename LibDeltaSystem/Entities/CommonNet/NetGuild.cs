using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetGuild : NetGuildSettings
    {
        public string icon_url;
        public string id;
        public string map_id;
        public string[] mods;
        public string content_server_hostname;
        public bool is_unconfigured;
        public DateTime last_secure_mode_toggled;

        public void SetServerData(DbServer server)
        {
            //Set
            name = server.display_name;
            icon_url = server.image_url;
            id = server.id;
            map_id = server.latest_server_map;
            last_secure_mode_toggled = server.last_secure_mode_toggled;
            permission_flags = server.permission_flags;
            permissions_template = server.permissions_template;
            mods = server.mods;
            content_server_hostname = server.game_content_server_hostname;
            is_locked = server.CheckFlag(DbServer.FLAG_INDEX_LOCKED);
            is_unconfigured = server.CheckFlag(DbServer.FLAG_INDEX_SETUP);
            is_secure = server.CheckFlag(DbServer.FLAG_INDEX_SECURE);
        }

        public static NetGuildUser GetGuild(DbServer server)
        {
            NetGuildUser g = new NetGuildUser();
            g.SetServerData(server);
            return g;
        }
    }
}
