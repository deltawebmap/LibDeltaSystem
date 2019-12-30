using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System.Entities;
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
        /// The machine this is connected to. Can be null if created using the mods.
        /// </summary>
        public string machine_uid { get; set; }

        /// <summary>
        /// ID of the owner of the server
        /// </summary>
        public string owner_uid { get; set; }

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
        /// Lock flags, in terms of bits. 0 if OK to load
        /// https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit?folder=0AOcXNqRr5p22Uk9PVA#gid=242769975 (only lower bits are used)
        /// </summary>
        public uint lock_flags { get; set; }

        /// <summary>
        /// Permissions, in terms of bits.
        /// https://docs.google.com/spreadsheets/d/1zQ_r86uyDAvwAtEg0135rL6g2lHqhPtYAgFdJrL3vZc/edit?folder=0AOcXNqRr5p22Uk9PVA#gid=0
        /// </summary>
        public ulong permission_flags { get; set; }

        /// <summary>
        /// Is this server a PVP server?
        /// </summary>
        public bool is_pvp { get; set; }

        /// <summary>
        /// Holds current revision IDs. Maximum of 32.
        /// </summary>
        public ulong[] revision_ids { get; set; } = new ulong[32];

        /// <summary>
        /// Multiplier for how quickly events are sent from the ARK server. Requires reboot. 1 is default. Increase for larger servers
        /// </summary>
        public float update_speed_multiplier { get; set; } = 1;

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
        /// Checks a lock flag at a bit index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckLockFlag(int index)
        {
            return ((lock_flags >> index) & 1U) == 1;
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
        /// Updates the listed permission flags.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public void SetPermissionFlags(bool[] points)
        {
            //Verify
            if (points.Length != 64)
                throw new Exception("Points array length does not match 64.");

            //Set
            for (int i = 0; i < 64; i++)
            {
                ulong value = 0;
                if (points[i])
                    value = 1;
                permission_flags |= ((value >> 0) & 1) << i;
            }
        }

        /// <summary>
        /// Sets a lock flag, but DOES NOT SAVE
        /// </summary>
        /// <param name="index"></param>
        /// <param name="flag"></param>
        public void SetLockFlag(int index, bool flag)
        {
            uint value = 0;
            if (flag)
                value = 1;
            lock_flags |= ((value >> 0) & 1) << index;
        }

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all canvases
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<DbCanvas>> GetServerCanvases()
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("server_id", id);
            var result = await conn.system_canvases.FindAsync(filter);
            List<DbCanvas> can = await result.ToListAsync();
            foreach (var c in can)
                c.conn = conn;
            return can;
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync()
        {
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_servers.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task ExplicitUpdateAsync(UpdateDefinition<DbServer> update)
        {
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_servers.FindOneAndUpdateAsync(filter, update);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_servers.FindOneAndDeleteAsync(filter);
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<DbPlayerProfile> GetPlayerProfileBySteamIdAsync(string steamId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", _id);
            var results = await conn.content_player_profiles.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns tribe data from it's ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<DbTribe> GetTribeAsync(int tribeId)
        {
            return await conn.GetTribeByTribeIdAsync(id, tribeId);
        }

        /// <summary>
        /// Uses your Steam ID to try and get your tribe ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<int?> TryGetTribeIdAsync(string steamId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", id) & filterBuilder.Eq("steam_id", steamId);
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
        public async Task<List<DbPlayerProfile>> GetPlayerProfilesByTribeAsync(int tribeId)
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
        public async Task<List<DbUser>> GetUsersByTribeAsync(int tribeId)
        {
            //Get all player profiles
            var profiles = await GetPlayerProfilesByTribeAsync(tribeId);

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

        /// <summary>
        /// Returns a placeholder icon
        /// </summary>
        /// <param name="display_name"></param>
        /// <returns></returns>
        public static string StaticGetPlaceholderIcon(string display_name)
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
            return "https://icon-assets.deltamap.net/legacy/placeholder_server_images/" + output + ".png";
        }

        public bool IsUserAdmin(DbUser user)
        {
            return owner_uid == user.id;
        }

        /// <summary>
        /// Gets user prefs for a saved user
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public async Task<SavedUserServerPrefs> GetUserPrefs(string user_id)
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
                    x = 128,
                    y = -128,
                    z = 2,
                    map = 0,
                    drawable_map = null
                };
            }
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
