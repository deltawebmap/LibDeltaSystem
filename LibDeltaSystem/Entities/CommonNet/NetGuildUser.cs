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
        public DbVector3 my_location; //The current location of the user
        public string ark_id;
        public bool is_admin;
        public bool has_tribe;

        public async Task SetServerGuildData(DeltaConnection conn, DbServer server, DbUser user, DbPlayerProfile profile)
        {
            //Set profile information
            if(profile != null)
            {
                //Get tribe info
                var tribe = await conn.GetTribeByTribeIdAsync(server._id, profile.tribe_id);
                has_tribe = tribe != null;
                if(tribe != null)
                {
                    target_tribe = tribe;
                }

                //Get my location
                DbVector3 myPos = null;
                if (profile.x != null && profile.y != null && profile.z != null)
                    myPos = new DbVector3
                    {
                        x = profile.x.Value,
                        y = profile.y.Value,
                        z = profile.z.Value
                    };

                //Set
                my_location = myPos;
                ark_id = profile.ark_id.ToString();
            } else
            {
                has_tribe = false;
            }
            
            is_admin = server.CheckIsUserAdmin(user);
            user_prefs = await server.GetUserPrefs(conn, user.id);
        }

        public static async Task<NetGuildUser> GetNetGuild(DeltaConnection conn, DbServer server, DbUser user, DbPlayerProfile profile)
        {
            NetGuildUser g = new NetGuildUser();
            await g.SetServerData(conn, server);
            await g.SetServerGuildData(conn, server, user, profile);
            return g;
        }

        public static async Task<NetGuildUser> GetNetGuild(DeltaConnection conn, DbServer server, DbUser user)
        {
            NetGuildUser g = new NetGuildUser();
            var profile = await server.GetUserPlayerProfile(conn, user);
            await g.SetServerData(conn, server);
            await g.SetServerGuildData(conn, server, user, profile);
            return g;
        }
    }
}
