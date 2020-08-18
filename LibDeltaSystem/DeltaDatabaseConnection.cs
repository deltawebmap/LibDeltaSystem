using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    /// <summary>
    /// A database-only version of DeltaConnection
    /// </summary>
    public class DeltaDatabaseConnection
    {
        private MongoClient content_client;
        private IMongoDatabase content_database;
        private IMongoDatabase system_database;
        private IMongoDatabase charlie_database;

        public IMongoCollection<DbDino> content_dinos;
        public IMongoCollection<DbInventory> content_inventories;
        public IMongoCollection<DbItem> content_items;
        public IMongoCollection<DbTribe> content_tribes;
        public IMongoCollection<DbPlayerProfile> content_player_profiles;
        public IMongoCollection<DbStructure> content_structures;
        public IMongoCollection<DbTribeLogEntry> content_tribe_log;

        public IMongoCollection<DbUser> system_users;
        public IMongoCollection<DbToken> system_tokens;
        public IMongoCollection<DbServer> system_servers;
        public IMongoCollection<DbSteamCache> system_steam_cache;
        public IMongoCollection<DbSteamModCache> system_steam_mod_cache;
        public IMongoCollection<DbPreregisteredUser> system_preregistered;
        public IMongoCollection<DbSavedUserServerPrefs> system_saved_user_server_prefs;
        public IMongoCollection<DbCanvas> system_canvases;
        public IMongoCollection<DbSyncSavedState> system_sync_states;
        public IMongoCollection<DbOauthApp> system_oauth_apps;
        public IMongoCollection<DbCluster> system_clusters;
        public IMongoCollection<DbQueuedSyncRequest> system_queued_sync_commands;
        public IMongoCollection<DbBetaKey> system_beta_keys;
        public IMongoCollection<DbAuthenticationSession> system_auth_sessions;
        public IMongoCollection<DbSystemServer> system_delta_servers;
        public IMongoCollection<DbSystemAdminUser> system_delta_admin_users;
        public IMongoCollection<DbServerPing> system_server_pings;

        public IMongoCollection<DbArkEntry<DinosaurEntry>> arkentries_dinos;
        public IMongoCollection<DbArkEntry<ItemEntry>> arkentries_items;
        public IMongoCollection<DbArkMapEntry> arkentries_maps;
        public IMongoCollection<DbPrimalPackage> arkentries_primal_packages;

        public static DeltaDatabaseConnection OpenFromDeltaConfig(string deltaConfigPath)
        {
            if (!File.Exists(deltaConfigPath))
                throw new Exception("Delta config file was not found! Requested location: " + deltaConfigPath);
            var config = JsonConvert.DeserializeObject<DeltaConnectionConfig>(File.ReadAllText(deltaConfigPath));
            var c = new DeltaDatabaseConnection();
            c.OpenDatabase(config.mongodb_connection, config.env);
            return c;
        }

        public void OpenDatabase(string mongoConnection, string mongoEnv)
        {
            content_client = new MongoClient(
                mongoConnection
            );

            content_database = content_client.GetDatabase("delta-" + mongoEnv + "-content");
            content_dinos = content_database.GetCollection<DbDino>("dinos");
            content_inventories = content_database.GetCollection<DbInventory>("inventories");
            content_items = content_database.GetCollection<DbItem>("items");
            content_tribes = content_database.GetCollection<DbTribe>("tribes");
            content_player_profiles = content_database.GetCollection<DbPlayerProfile>("player_profiles");
            content_structures = content_database.GetCollection<DbStructure>("structures");
            content_tribe_log = content_database.GetCollection<DbTribeLogEntry>("tribe_log_entries");

            system_database = content_client.GetDatabase("delta-" + mongoEnv + "-system");
            system_users = system_database.GetCollection<DbUser>("users");
            system_tokens = system_database.GetCollection<DbToken>("tokens");
            system_servers = system_database.GetCollection<DbServer>("servers");
            system_steam_cache = system_database.GetCollection<DbSteamCache>("steam_cache");
            system_steam_mod_cache = system_database.GetCollection<DbSteamModCache>("steam_mod_cache");
            system_preregistered = system_database.GetCollection<DbPreregisteredUser>("preregistered_users");
            system_saved_user_server_prefs = system_database.GetCollection<DbSavedUserServerPrefs>("saved_user_server_prefs");
            system_canvases = system_database.GetCollection<DbCanvas>("canvases");
            system_sync_states = system_database.GetCollection<DbSyncSavedState>("sync_states");
            system_oauth_apps = system_database.GetCollection<DbOauthApp>("oauth_apps");
            system_clusters = system_database.GetCollection<DbCluster>("clusters");
            system_queued_sync_commands = system_database.GetCollection<DbQueuedSyncRequest>("queued_sync_commands");
            system_beta_keys = system_database.GetCollection<DbBetaKey>("beta_keys");
            system_auth_sessions = system_database.GetCollection<DbAuthenticationSession>("auth_sessions");
            system_delta_servers = system_database.GetCollection<DbSystemServer>("delta_servers");
            system_delta_admin_users = system_database.GetCollection<DbSystemAdminUser>("delta_admin_users");
            system_server_pings = system_database.GetCollection<DbServerPing>("server_pings");

            charlie_database = content_client.GetDatabase("delta-" + mongoEnv + "-charlie");
            arkentries_dinos = charlie_database.GetCollection<DbArkEntry<DinosaurEntry>>("dino_entries");
            arkentries_items = charlie_database.GetCollection<DbArkEntry<ItemEntry>>("item_entries");
            arkentries_maps = charlie_database.GetCollection<DbArkMapEntry>("maps");
            arkentries_primal_packages = charlie_database.GetCollection<DbPrimalPackage>("packages");
        }

        private static async Task<T> GetDocumentById<T>(IMongoCollection<T> collec, string id)
        {
            return await GetDocumentById<T>(collec, ObjectId.Parse(id));
        }

        private static async Task<T> GetDocumentById<T>(IMongoCollection<T> collec, ObjectId id)
        {
            //Find
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var results = await collec.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            return r;
        }

        private static async Task<bool> DeleteDocumentById<T>(IMongoCollection<T> collec, ObjectId id)
        {
            //Find
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var results = await collec.DeleteOneAsync(filter);
            return results.DeletedCount == 1;
        }

        public static async Task<bool> UpdateDocumentById<T>(IMongoCollection<T> collec, ObjectId id, UpdateDefinition<T> update)
        {
            //Find
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var results = await collec.UpdateOneAsync(filter, update);
            return results.MatchedCount == 1;
        }

        /// <summary>
        /// Gets an ARK map by it's internal ARK name (for example, "ScorchedEarth_P")
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ArkMapEntry> GetARKMapByInternalName(string id)
        {
            var filterBuilder = Builders<DbArkMapEntry>.Filter;
            var filter = filterBuilder.Eq("internalName", id);
            var results = await arkentries_maps.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;
            else
                return r.data;
        }

        /// <summary>
        /// Gets all ARK maps
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, ArkMapEntry>> GetARKMaps()
        {
            var results = await arkentries_maps.FindAsync(FilterDefinition<DbArkMapEntry>.Empty);
            var resultsList = await results.ToListAsync();
            Dictionary<string, ArkMapEntry> output = new Dictionary<string, ArkMapEntry>();
            foreach (var r in resultsList)
                output.Add(r.internalName, r.data);
            return output;
        }

        /// <summary>
        /// Gets a user content entry by it's token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbOauthApp> GetOAuthAppByAppID(string id)
        {
            var filterBuilder = Builders<DbOauthApp>.Filter;
            var filter = filterBuilder.Eq("client_id", id);
            var result = await system_oauth_apps.FindAsync(filter);
            DbOauthApp c = await result.FirstOrDefaultAsync();
            if (c == null)
                return null;
            return c;
        }

        public async Task<DbOauthApp> GetOAuthAppByInternalID(ObjectId id)
        {
            var filterBuilder = Builders<DbOauthApp>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var result = await system_oauth_apps.FindAsync(filter);
            DbOauthApp c = await result.FirstOrDefaultAsync();
            if (c == null)
                return null;
            return c;
        }

        /// <summary>
        /// Loads canvas data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbCanvas> LoadCanvasData(ObjectId id)
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var result = await system_canvases.FindAsync(filter);
            DbCanvas c = await result.FirstOrDefaultAsync();
            if (c == null)
                return null;
            return c;
        }

        /// <summary>
        /// Returns a user by their user ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbUser> GetUserByIdAsync(string id)
        {
            //Fetch
            DbUser u = await GetDocumentById<DbUser>(system_users, id);

            //If not found, return null
            if (u == null)
                return null;

            return u;
        }

        /// <summary>
        /// Returns a user by their user ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbUser> GetUserByIdAsync(ObjectId id)
        {
            //Fetch
            DbUser u = await GetDocumentById<DbUser>(system_users, id);

            //If not found, return null
            if (u == null)
                return null;

            return u;
        }

        /// <summary>
        /// Returns all pending sync requests
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<DbQueuedSyncRequest>> GetQueuedSyncCommandsForServerById(ObjectId id, bool clear, int limit)
        {
            var filterBuilder = Builders<DbQueuedSyncRequest>.Filter;
            var filter = filterBuilder.Eq("server_id", id);
            var results = await system_queued_sync_commands.FindAsync(filter, new FindOptions<DbQueuedSyncRequest, DbQueuedSyncRequest>
            {
                Limit = limit
            });
            var resultList = await results.ToListAsync();
            if (clear)
            {
                foreach (var r in resultList)
                {
                    await DeleteDocumentById(system_queued_sync_commands, r._id);
                }
            }
            return resultList;
        }

        /// <summary>
        /// Returns a user by their user ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbServer> GetServerByIdAsync(string id)
        {
            return await GetServerByIdAsync(ObjectId.Parse(id));
        }

        /// <summary>
        /// Returns a user by their user ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbServer> GetServerByIdAsync(ObjectId id)
        {
            //Fetch
            DbServer u = await GetDocumentById<DbServer>(system_servers, id);

            //If not found, return null
            if (u == null)
                return null;

            return u;
        }

        /// <summary>
        /// Returns a server by their ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<DbServer>> GetServersByOwnerAsync(string id)
        {
            //Check if null
            if (id == null)
                return new List<DbServer>();

            //Fetch
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("owner_uid", id);
            var results = await system_servers.FindAsync(filter);
            var r = await results.ToListAsync();

            return r;
        }

        /// <summary>
        /// Returns a token object from the token string
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUser> GetUserBySteamIdAsync(string steamid)
        {
            //Find
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("steam_id", steamid);
            var results = await system_users.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;

            return r;
        }

        /// <summary>
        /// Returns a token object from the token string
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbToken> GetTokenByTokenAsync(string token)
        {
            //Find
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var results = await system_tokens.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;

            return r;
        }

        /// <summary>
        /// Returns a token object from the token string
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUser> GetUserByServerSetupToken(string token)
        {
            if (token == null)
                return null;

            //Find
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("server_creation_token", token);
            var results = await system_users.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;

            return r;
        }

        /// <summary>
        /// Authenticates a user with a token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUser> AuthenticateUserToken(string token)
        {
            return await DbUser.AuthenticateUserToken(this, token);
        }

        /// <summary>
        /// Authenticates a server with it's token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbServer> AuthenticateServerTokenAsync(string token)
        {
            //Find the machine in the database
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var results = await system_servers.FindAsync(filter);
            var s = await results.FirstOrDefaultAsync();
            if (s == null)
                return null;
            return s;
        }

        /// <summary>
        /// Authenticates a server with it's token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbSyncSavedState> AuthenticateServerSessionTokenAsync(string token)
        {
            //Find the machine in the database
            var filterBuilder = Builders<DbSyncSavedState>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var results = await system_sync_states.FindAsync(filter);
            var s = await results.FirstOrDefaultAsync();
            if (s == null)
                return null;
            return s;
        }

        /// <summary>
        /// Returns tribe data from it's ID
        /// </summary>
        /// <param name="steamId"></param>
        /// <returns></returns>
        public async Task<DbTribe> GetTribeByTribeIdAsync(ObjectId serverId, int tribeId)
        {
            var filterBuilder = Builders<DbTribe>.Filter;
            var filter = filterBuilder.Eq("server_id", serverId) & filterBuilder.Eq("tribe_id", tribeId);
            var results = await content_tribes.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            if (r == null)
                return null;
            else
                return r;
        }

        public async Task<DbPrimalPackage> GetPrimalPackageByNameAsync(string name)
        {
            var filterBuilder = Builders<DbPrimalPackage>.Filter;
            var filter = filterBuilder.Eq("name", name);
            var results = await arkentries_primal_packages.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }

        public async Task<DbPrimalPackage> GetPrimalPackageByModAsync(long mod, string type)
        {
            var filterBuilder = Builders<DbPrimalPackage>.Filter;
            var filter = filterBuilder.Eq("mod_id", mod) & filterBuilder.Eq("package_type", type);
            var results = await arkentries_primal_packages.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }
    }
}
