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
        /// Returns the player profile by ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<DbPlayerProfile> GetPlayerProfileBySteamIDAsync(DeltaConnection conn, int? tribeId, string steamId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("steam_id", steamId) & Tools.FilterBuilderToolDb.CreateTribeFilter<DbPlayerProfile>(this, tribeId);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

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

        public async Task<ArkMapEntry> GetMapEntryAsync(DeltaConnection conn)
        {
            return await conn.GetARKMapByInternalName(latest_server_map);
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
        /// Returns all permission flags as an array of bools
        /// </summary>
        /// <returns></returns>
        public bool[] GetPermissionFlagList()
        {
            bool[] response = new bool[64];
            for (int i = 0; i < response.Length; i++)
                response[i] = CheckPermissionFlag(i);
            return response;
        }

        /// <summary>
        /// Gets all canvases
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task ExplicitUpdateAsync(DeltaConnection conn, UpdateDefinition<DbServer> update)
        {
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_servers.FindOneAndUpdateAsync(filter, update);
        }

        /// <summary>
        /// Returns the player profile for a user, if any
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<DbPlayerProfile> GetUserPlayerProfile(DeltaConnection conn, DbUser user)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("steam_id", user.steam_id);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the player profile for a user, if any
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUserPlayerProfile(DeltaConnection conn, DbUser user)
        {
            ///Remove
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id) & filterBuilder.Eq("steam_id", user.steam_id);
            var results = await conn.content_player_profiles.DeleteOneAsync(filter);

            //Notify of the person leaving if they aren't admin or owner
            if(!admins.Contains(user._id))
                await NotifyUserRemoved(conn, user);

            //Reset user groups
            RPCMessageTool.SystemNotifyUserGroupReset(conn, user);

            return results.DeletedCount == 1;
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns tribe data from it's ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Uses your Steam ID to try and get your tribe ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<List<DbPlayerProfile>> GetPlayerProfilesByTribeAsync(DeltaConnection conn, int tribeId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", id) & filterBuilder.Eq("tribe_id", tribeId);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
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

        public string[] GetAllServerModIDs()
        {
            //Verify
            if (game_settings == null)
                return new string[0];
            if (game_settings.ActiveMods == null)
                return new string[0];
            return game_settings.ActiveMods.Split(',');
        }

        public async Task<Dictionary<string, DbSteamModCache>> GetAllServerMods(DeltaConnection conn, bool includeFailed)
        {
            //Get mod IDs
            string[] ids = GetAllServerModIDs();
            
            //Get all
            Dictionary<string, DbSteamModCache> dict = new Dictionary<string, DbSteamModCache>();
            foreach(var s in ids)
            {
                var data = await conn.GetSteamModById(s);
                if (data != null)
                    dict.Add(s, data);
                else if (includeFailed)
                    dict.Add(s, null);
            }

            return dict;
        }

        /// <summary>
        /// Returns a placeholder icon
        /// </summary>
        /// <param name="display_name"></param>
        /// <returns></returns>
        public static string StaticGetPlaceholderIcon(DeltaConnection conn, string display_name)
        {
            //Find letters
            string[] words = display_name.Split(' ');
            char[] charset = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray();
            string output = "";
            for (int i = 0; i < words.Length; i++)
            {
                if (output.Length >= 2)
                    break;
                if (words[i].Length > 1)
                {
                    char c = words[i][0];
                    if (charset.Contains(c))
                    {
                        string sc = new string(new char[] { c });
                        if (output.Length == 0)
                            sc = sc.ToUpper();
                        else
                            sc = sc.ToLower();
                        output += sc;
                    }
                }
            }

            //Now, return URL
            return conn.config.hosts.assets_icon + "/legacy/placeholder_server_images/" + output + ".png";
        }

        public bool IsUserAdmin(DbUser user)
        {
            return CheckIsUserAdmin(user);
        }

        /// <summary>
        /// Gets user prefs for a saved user
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public async Task<SavedUserServerPrefs> GetUserPrefs(DeltaConnection conn, string user_id)
        {
            var filterBuilder = Builders<DbSavedUserServerPrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", id) & filterBuilder.Eq("user_id", user_id);
            var results = conn.system_saved_user_server_prefs.Find(filter).FirstOrDefault();
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

        /// <summary>
        /// Updates the server and automatically sends RPC events
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        public async Task UpdateAsync(DeltaConnection conn, UpdateDefinition<DbServer> update)
        {
            //Send update
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            var result = await conn.system_servers.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<DbServer, DbServer>
            {
                ReturnDocument = ReturnDocument.After
            });

            //Send RPC events
            await Tools.RPCMessageTool.SendGuildUpdate(conn, result);
        }

        /// <summary>
        /// Gets the cluster for this server
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public async Task<DbCluster> GetClusterAsync(DeltaConnection conn)
        {
            if (cluster_id == null)
                return null;
            return await DbCluster.GetClusterById(conn, ObjectId.Parse(cluster_id));
        }

        public async Task ChangeSecureMode(DeltaConnection conn, bool secure)
        {
            //Update
            await ExplicitUpdateAsync(conn, Builders<DbServer>.Update.Set("secure_mode", secure).Set("last_secure_mode_toggled", DateTime.UtcNow));

            //Send RPC message
            RPCMessageTool.SendGuildSetSecureMode(conn, this, secure);
        }

        public async Task ChangePermissionFlags(DeltaConnection conn, int flags)
        {
            //Update
            this.permission_flags = flags;
            await ExplicitUpdateAsync(conn, Builders<DbServer>.Update.Set("permission_flags", flags));

            //Send RPC message
            RPCMessageTool.SendGuildPermissionChanged(conn, this);
        }

        public async Task<bool> RemoveAdmin(DeltaConnection conn, DbUser user)
        {
            //Change admins
            if (!admins.Contains(user._id))
                return false;
            admins.Remove(user._id);
            
            //Update
            await ExplicitUpdateAsync(conn, Builders<DbServer>.Update.Set("admins", admins));

            //Send RPC message
            RPCMessageTool.SendUserServerPermissionsChanged(conn, user._id, this); //Tell this user about the change
            RPCMessageTool.SendGuildAdminListUpdated(conn, this); //Tell users on the server
            RPCMessageTool.SystemNotifyUserGroupReset(conn, user); //Reset user groups

            //If this user doesn't have a profile and isn't owner, that means that they've lost access. Tell them that
            if (await GetUserPlayerProfile(conn, user) == null)
                await NotifyUserRemoved(conn, user);

            return true;
        }

        public async Task NotifyUserRemoved(DeltaConnection conn, DbUser user)
        {
            RPCMessageTool.SendUserServerRemoved(conn, user._id, this);
            RPCMessageTool.SendGuildUserRemoved(conn, this, user);
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

            //Send RPC event to all
            RPCMessageTool.SendGuildServerRemoved(conn, this);
        }

        public async Task NotifyPublicDetailsChanged(DeltaConnection conn)
        {
            await RPCMessageTool.SendGuildPublicDetailsChanged(conn, this);
        }
    }

    /// <summary>
    /// Settings for local clients
    /// </summary>
    public class DbServer_LoadSettings
    {
        /// <summary>
        /// The pathname to the save directory. Always ends with / or \.
        /// </summary>
        public string save_pathname { get; set; }

        /// <summary>
        /// The .ark file to load, relative to save_pathname. Example: "Extinction.ark"
        /// </summary>
        public string save_map_name { get; set; }

        /// <summary>
        /// Path to the config name.
        /// </summary>
        public string config_pathname { get; set; }
    }
}
