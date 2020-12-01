using LibDeltaSystem.Db;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetGuildUser : NetGuild
    {
        public SavedUserServerPrefs user_prefs;
        public DbTribe target_tribe; //The tribe this user belongs to
        public bool is_admin;

        public async Task SetServerGuildData(DeltaConnection conn, DbServer server, DbUser user, DbPlayerProfile profile)
        {
            //Get tribe info
            target_tribe = await conn.GetTribeByTribeIdAsync(server._id, profile.tribe_id);

            //Set other
            is_admin = server.CheckIsUserAdmin(user);
            user_prefs = await server.GetUserPrefs(conn, user.id);
        }

        public static async Task<NetGuildUser> GetNetGuild(DeltaConnection conn, DbServer server, DbUser user, DbPlayerProfile profile)
        {
            NetGuildUser g = new NetGuildUser();
            g.SetServerData(server);
            await g.SetServerGuildData(conn, server, user, profile);
            return g;
        }

        public static async Task<NetGuildUser> GetNetGuild(DeltaConnection conn, DbServer server, DbUser user)
        {
            NetGuildUser g = new NetGuildUser();
            var profile = await server.GetUserPlayerProfile(conn, user);
            g.SetServerData(server);
            await g.SetServerGuildData(conn, server, user, profile);
            return g;
        }
    }
}
