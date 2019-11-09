using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System.Entities;
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
        /// The settings used to load the map data.
        /// </summary>
        public DbServer_LoadSettings load_settings { get; set; }

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
        /// Files for echo sync
        /// </summary>
        public List<ServerEchoUploadedFile> echo_files { get; set; }

        /// <summary>
        /// Revision ID for dinos
        /// </summary>
        public int revision_id_dinos { get; set; }

        /// <summary>
        /// Revision ID for structures
        /// </summary>
        public int revision_id_structures { get; set; }

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
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
