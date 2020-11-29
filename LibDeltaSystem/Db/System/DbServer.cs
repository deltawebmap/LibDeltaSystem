using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Tools;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbServer : DbBaseSystem
    {
        /// <summary>
        /// Name shown in the UI
        /// </summary>
        public string display_name { get; set; }

        /// <summary>
        /// URL to a server icon.
        /// </summary>
        public string image_url { get; set; }

        /// <summary>
        /// Has a custom icon
        /// </summary>
        public bool has_custom_image { get; set; }

        /// <summary>
        /// Creds checked to verify the connection between the slave server.
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The latest map the server was on.
        /// </summary>
        public string latest_server_map { get; set; }

        /// <summary>
        /// Latest time of the Ark server
        /// </summary>
        public float latest_server_time { get; set; }

        /// <summary>
        /// The linked cluster ID. Can be null if this is not in a cluster.
        /// </summary>
        public string cluster_id { get; set; }

        /// <summary>
        /// Game config settings
        /// </summary>
        public DbServerGameSettings game_settings { get; set; }

        /// <summary>
        /// Server mods
        /// </summary>
        public string[] mods { get; set; }

        /// <summary>
        /// Permissions, in terms of bits.
        /// https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit?folder=0AOcXNqRr5p22Uk9PVA#gid=0
        /// </summary>
        public int permission_flags { get; set; }

        /// <summary>
        /// Is this server a PVP server?
        /// </summary>
        public bool is_pvp { get; set; }

        /// <summary>
        /// Multiplier for how quickly events are sent from the ARK server. Requires reboot. 1 is default. Increase for larger servers
        /// </summary>
        public float update_speed_multiplier { get; set; } = 1;

        /// <summary>
        /// Delta Web Map user IDs with admin access. Does not include owner UID
        /// </summary>
        public List<ObjectId> admins { get; set; } = new List<ObjectId>();

        /// <summary>
        /// In secure mode, admins cannot access other tribes data. This can help to ensure tribes that there is no admin abuse
        /// </summary>
        public bool secure_mode { get; set; }

        /// <summary>
        /// The last time secure mode was toggled. Used to notify users if it's been changed
        /// </summary>
        public DateTime last_secure_mode_toggled { get; set; }

        /// <summary>
        /// State flags https://docs.google.com/spreadsheets/d/1DcpgobdkajnkzJi98Gml8hG4Ok4hkBJoO7ylpnG_57Y/edit?usp=sharing
        /// </summary>
        public int flags { get; set; }

        /// <summary>
        /// The permissions setup template to use
        /// </summary>
        public string permissions_template { get; set; } = "NORMAL";

        /// <summary>
        /// Last time this was connected to by the sync server
        /// </summary>
        public DateTime last_connected_time { get; set; }

        /// <summary>
        /// Last time this was pinged by the sync server
        /// </summary>
        public DateTime last_pinged_time { get; set; }

        /// <summary>
        /// The ID of the last sync state
        /// </summary>
        public ObjectId last_sync_state { get; set; }

        /// <summary>
        /// The version the last ARK client that contacted this server was running
        /// </summary>
        public int last_client_version { get; set; }

        /// <summary>
        /// The hostname of the server responsible for hosting ARK content
        /// </summary>
        public string game_content_server_hostname { get; set; } = "us-01.content-prod.deltamap.net";

        /// <summary>
        /// Checks if a user is admin on this server
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckIsUserAdmin(DbUser user)
        {
            return admins.Contains(user._id);
        }

        /// <summary>
        /// Checks if a user is admin on this server
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool CheckIsUserAdmin(ObjectId user)
        {
            return admins.Contains(user);
        }

        /// <summary>
        /// Checks a permission flag at a bit index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckPermissionFlag(int index)
        {
            return ((permission_flags >> index) & 1U) == 1;
        }

        /// <summary>
        /// Chcks a flag at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckFlag(int index)
        {
            return ((flags >> index) & 1U) == 1;
        }

        public const int FLAG_INDEX_LOCKED = 0;
        public const int FLAG_INDEX_SETUP = 1;

        public async Task<DbPlayerProfile> GetPlayerProfileBySteamIDAsync(DeltaConnection conn, int? tribeId, string steamId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("steam_id", steamId) & Tools.FilterBuilderToolDb.CreateTribeFilter<DbPlayerProfile>(this, tribeId);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

        public async Task<List<DbCanvas>> GetServerCanvases(DeltaConnection conn, int? tribe_id)
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            FilterDefinition<DbCanvas> filter;
            if(!tribe_id.HasValue)
                filter = filterBuilder.Eq("server_id", _id);
            else
                filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("tribe_id", tribe_id.Value);
            var result = await conn.system_canvases.FindAsync(filter);
            List<DbCanvas> can = await result.ToListAsync();
            return can;
        }

        public async Task<DbPlayerProfile> GetUserPlayerProfile(DeltaConnection conn, DbUser user)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("steam_id", user.steam_id);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

        public async Task<List<DbPlayerProfile>> GetPlayerProfiles(DeltaConnection conn, int offset = 0, int limit = int.MaxValue)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id);
            var results = await conn.content_player_profiles.FindAsync(filter, new FindOptions<DbPlayerProfile, DbPlayerProfile>
            {
                Skip = offset,
                Limit = limit
            });
            return await results.ToListAsync();
        }

        public async Task<DbTribe> GetTribeAsync(DeltaConnection conn, int tribeId)
        {
            return await conn.GetTribeByTribeIdAsync(_id, tribeId);
        }

        public async Task<List<DbTribe>> GetAllTribesAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbTribe>.Filter;
            var filter = filterBuilder.Eq("server_id", this._id);
            var results = await conn.content_tribes.FindAsync(filter);
            var r = await results.ToListAsync();
            return r;
        }

        public async Task<int?> TryGetTribeIdAsync(DeltaConnection conn, string steamId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("steam_id", steamId);
            var results = await conn.content_player_profiles.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;
            else
                return r.tribe_id;
        }

        public async Task<List<DbPlayerProfile>> GetPlayerProfilesByTribeAsync(DeltaConnection conn, int tribeId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", id) & filterBuilder.Eq("tribe_id", tribeId);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.ToListAsync();
        }

        public async Task<List<DbUser>> GetUsersByTribeAsync(DeltaConnection conn, int tribeId)
        {
            //Get all player profiles
            var profiles = await GetPlayerProfilesByTribeAsync(conn, tribeId);

            //Now, get all DbUsers
            List<DbUser> users = new List<DbUser>();
            foreach(var p in profiles)
            {
                var u = await conn.GetUserBySteamIdAsync(p.steam_id);
                if (u != null)
                    users.Add(u);
            }

            return users;
        }

        public async Task<SavedUserServerPrefs> GetUserPrefs(DeltaConnection conn, string user_id)
        {
            var filterBuilder = Builders<DbSavedUserServerPrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", id) & filterBuilder.Eq("user_id", user_id);
            var results = (await conn.system_saved_user_server_prefs.FindAsync(filter)).FirstOrDefault();
            if (results != null)
            {
                return results.payload;
            }
            else
            {
                return new SavedUserServerPrefs
                {
                    saved_map_pos = new DbVector3
                    {
                        x = 0,
                        y = 0,
                        z = 2
                    }
                };
            }
        }

        public async Task<bool> DeleteUserPlayerProfile(DeltaConnection conn, DbUser user)
        {
            ///Remove
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("steam_id", user.steam_id);
            var results = await conn.content_player_profiles.DeleteOneAsync(filter);

            //Send Events
            await conn.events.OnUserServerAccessChangedAsync(this, user); //Tell the user
            conn.events.NotifyUserGroupsUpdated(user._id); //Notify the system that their permissions were changed

            return results.DeletedCount == 1;
        }

        public async Task DeleteServer(DeltaConnection conn)
        {
            //Delete this
            var filter = Builders<DbServer>.Filter.Eq("_id", this._id);
            await conn.system_servers.DeleteOneAsync(filter);

            //Delete content
            await DbTribe.DeleteServerContent(conn, this._id);
            await DbStructure.DeleteServerContent(conn, this._id);
            await DbInventory.DeleteServerContent(conn, this._id);
            await DbDino.DeleteServerContent(conn, this._id);
            await DbPlayerProfile.DeleteServerContent(conn, this._id);

            //Send Events
            conn.events.OnServerDeleted(this);
            //It's unimportant to notify user groups changed, as no further events will ever be sent from this server
        }

        public Updaters.DbServerUpdateBuilder GetUpdateBuilder(DeltaConnection conn)
        {
            return new Updaters.DbServerUpdateBuilder(conn, this);
        }
    }
}
