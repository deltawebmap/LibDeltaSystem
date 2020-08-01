﻿using LibDeltaSystem.CoreHub;
using LibDeltaSystem.CoreHub.CoreNetwork;
using LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Analytics;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Entities.DynamicTiles;
using LibDeltaSystem.Entities.PrivateNet;
using LibDeltaSystem.Tools;
using LibDeltaSystem.WebFramework;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    public class DeltaConnection
    {
        public const byte LIB_VERSION_MAJOR = 0;
        public const byte LIB_VERSION_MINOR = 6;
        
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
        public IMongoCollection<DbEgg> content_eggs;

        public IMongoCollection<DbUser> system_users;
        public IMongoCollection<DbToken> system_tokens;
        public IMongoCollection<DbServer> system_servers;
        public IMongoCollection<DbMachine> system_machines;
        public IMongoCollection<DbSteamCache> system_steam_cache;
        public IMongoCollection<DbSteamModCache> system_steam_mod_cache;
        public IMongoCollection<DbErrorLog> system_error_log;
        public IMongoCollection<DbPreregisteredUser> system_preregistered;
        public IMongoCollection<DbSavedUserServerPrefs> system_saved_user_server_prefs;
        public IMongoCollection<DbSavedDinoTribePrefs> system_saved_dino_tribe_prefs;
        public IMongoCollection<DbDynamicTileCache> system_dynamic_tile_cache;
        public IMongoCollection<DbCanvas> system_canvases;
        public IMongoCollection<DbUserContent> system_user_uploads;
        public IMongoCollection<DbSyncSavedState> system_sync_states;
        public IMongoCollection<DbOauthApp> system_oauth_apps;
        public IMongoCollection<DbCluster> system_clusters;
        public IMongoCollection<DbModTimeAnalyticsObject> system_analytics_time;
        public IMongoCollection<DbAlertBanner> system_alert_banners;
        public IMongoCollection<DbQueuedSyncRequest> system_queued_sync_commands;
        public IMongoCollection<DbBetaKey> system_beta_keys;
        public IMongoCollection<DbAuthenticationSession> system_auth_sessions;
        public IMongoCollection<DbSystemServer> system_delta_servers;
        public IMongoCollection<DbSystemAdminUser> system_delta_admin_users;

        public IMongoCollection<DbArkEntry<DinosaurEntry>> arkentries_dinos;
        public IMongoCollection<DbArkEntry<ItemEntry>> arkentries_items;
        public IMongoCollection<DbArkMapEntry> arkentries_maps;

        public DeltaConnectionConfig config;

        public HttpClient http;

        public ushort server_id;
        public byte system_version_minor;
        public byte system_version_major;
        public BaseClientCoreNetwork network;
        public DbSystemServer me;
        public DateTime start_time;
        public Random rand;
        public DeltaWebServer web_server;

        [Obsolete("This method is simply for backwards compatibilty. Please do not identify servers by their string name.")]
        public string system_name { get { return network.me.id.ToString() + "~" + network.me.type.ToString(); } }

        public bool debug_mode { get { return config.debug_mode; } }

        public DeltaConnection(string pathname, ushort server_id, byte system_version_major, byte system_version_minor, BaseClientCoreNetwork network)
        {
            if (!File.Exists(pathname))
                throw new Exception("Delta config file was not found! Requested location: "+pathname);
            config = JsonConvert.DeserializeObject<DeltaConnectionConfig>(File.ReadAllText(pathname));
            this.http = new HttpClient();
            this.system_version_major = system_version_major;
            this.system_version_minor = system_version_minor;
            this.server_id = server_id;
            this.network = network;
            this.start_time = DateTime.UtcNow;
            this.rand = new Random();
        }

        /// <summary>
        /// Creates a managed app. StartupArgs are from the args used to start the application. Works with args passed by this bring run by a proccess manager. Connects too
        /// </summary>
        /// <param name="startupArgs"></param>
        /// <param name="system_version_major"></param>
        /// <param name="system_version_minor"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        public static DeltaConnection InitDeltaManagedApp(string[] startupArgs, byte system_version_major, byte system_version_minor, BaseClientCoreNetwork network)
        {
            //Write
            Console.WriteLine($"[LibDeltaSystem] Starting Delta managed app on version {system_version_major}.{system_version_minor} (lib version {LIB_VERSION_MAJOR}.{LIB_VERSION_MINOR})");
            
            //Validate
            if (startupArgs.Length != 2)
                throw new Exception("The startup args are not valid. This program is supposed to be run from a Delta Process Manager.");

            //Create
            var d = new DeltaConnection(startupArgs[0], ushort.Parse(startupArgs[1]), system_version_major, system_version_minor, network);

            //Connect
            d.Connect().GetAwaiter().GetResult();

            return d;
        }

        public int GetUserPort(int index)
        {
            return me.ports[index];
        }

        /// <summary>
        /// Connects and sets up databases
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            //Set up database
            content_client = new MongoClient(
                config.mongodb_connection
            );

            content_database = content_client.GetDatabase("delta-"+config.env+"-content");
            content_dinos = content_database.GetCollection<DbDino>("dinos");
            content_inventories = content_database.GetCollection<DbInventory>("inventories");
            content_items = content_database.GetCollection<DbItem>("items");
            content_tribes = content_database.GetCollection<DbTribe>("tribes");
            content_player_profiles = content_database.GetCollection<DbPlayerProfile>("player_profiles");
            content_structures = content_database.GetCollection<DbStructure>("structures");
            content_tribe_log = content_database.GetCollection<DbTribeLogEntry>("tribe_log_entries");
            content_eggs = content_database.GetCollection<DbEgg>("eggs");

            system_database = content_client.GetDatabase("delta-" + config.env + "-system");
            system_users = system_database.GetCollection<DbUser>("users");
            system_tokens = system_database.GetCollection<DbToken>("tokens");
            system_servers = system_database.GetCollection<DbServer>("servers");
            system_machines = system_database.GetCollection<DbMachine>("machines");
            system_steam_cache = system_database.GetCollection<DbSteamCache>("steam_cache");
            system_steam_mod_cache = system_database.GetCollection<DbSteamModCache>("steam_mod_cache");
            system_error_log = system_database.GetCollection<DbErrorLog>("error_log");
            system_preregistered = system_database.GetCollection<DbPreregisteredUser>("preregistered_users");
            system_saved_user_server_prefs = system_database.GetCollection<DbSavedUserServerPrefs>("saved_user_server_prefs");
            system_saved_dino_tribe_prefs = system_database.GetCollection<DbSavedDinoTribePrefs>("saved_dino_tribe_prefs");
            system_dynamic_tile_cache = system_database.GetCollection<DbDynamicTileCache>("dynamic_tile_cache");
            system_canvases = system_database.GetCollection<DbCanvas>("canvases");
            system_user_uploads = system_database.GetCollection<DbUserContent>("user_uploads");
            system_sync_states = system_database.GetCollection<DbSyncSavedState>("sync_states");
            system_oauth_apps = system_database.GetCollection<DbOauthApp>("oauth_apps");
            system_clusters = system_database.GetCollection<DbCluster>("clusters");
            system_analytics_time = system_database.GetCollection<DbModTimeAnalyticsObject>("analytics_time");
            system_alert_banners = system_database.GetCollection<DbAlertBanner>("alert_banners");
            system_queued_sync_commands = system_database.GetCollection<DbQueuedSyncRequest>("queued_sync_commands");
            system_beta_keys = system_database.GetCollection<DbBetaKey>("beta_keys");
            system_auth_sessions = system_database.GetCollection<DbAuthenticationSession>("auth_sessions");
            system_delta_servers = system_database.GetCollection<DbSystemServer>("delta_servers");
            system_delta_admin_users = system_database.GetCollection<DbSystemAdminUser>("delta_admin_users");

            charlie_database = content_client.GetDatabase("delta-" + config.env + "-charlie");
            arkentries_dinos = charlie_database.GetCollection<DbArkEntry<DinosaurEntry>>("dino_entries");
            arkentries_items = charlie_database.GetCollection<DbArkEntry<ItemEntry>>("item_entries");
            arkentries_maps = charlie_database.GetCollection<DbArkMapEntry>("maps");

            //Set up the core network
            var serverList = new CoreNetworkServerListDatabase();
            await serverList.Init(this, config.env);
            network.Init(this, server_id, serverList);

            //Set me
            if (serverList.me == null)
                throw new Exception("Could not find my own server ID in the server list!");
            else
                me = serverList.me;
        }

        public void Log(string topic, string message, DeltaLogLevel level)
        {
            //Translate level to console color
            switch(level)
            {
                case DeltaLogLevel.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
                case DeltaLogLevel.Low: Console.ForegroundColor = ConsoleColor.White; break;
                case DeltaLogLevel.Medium: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case DeltaLogLevel.High: Console.ForegroundColor = ConsoleColor.Red; break;
                case DeltaLogLevel.Alert: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Red; break;
            }

            //Log
            Console.WriteLine($"[{topic}] {message}");

            //Reset
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            //Remote log
            network.RemoteLog(topic, message, level);
        }

        public void AttachWebServer(DeltaWebServer server)
        {
            this.web_server = server;
        }

        /// <summary>
        /// Combines many small queries into one large one, then returns the mapped results
        /// </summary>
        /// <typeparam name="T">The type of data returned</typeparam>
        /// <typeparam name="K">The type of the field used to access this</typeparam>
        /// <returns></returns>
        public static async Task<Dictionary<K, T>> MassGetObjects<T, K>(IMongoCollection<T> collec, FilterDefinition<T> query, string fieldName, List<K> targets)
        {
            //Get raw
            var data = await MassGetObjectsRaw<T, K>(collec, query, fieldName, targets);

            //Do reflection stuff
            var field = typeof(T).GetField(fieldName);

            //Organize
            Dictionary<K, T> response = new Dictionary<K, T>();
            foreach (var d in data) {
                response.Add((K)field.GetValue(d), d);
            }

            return response;
        }

        /// <summary>
        /// Combines many small queries into one large one, then returns the raw results
        /// </summary>
        /// <typeparam name="T">The type of data returned</typeparam>
        /// <typeparam name="K">The type of the field used to access this</typeparam>
        /// <returns></returns>
        public static async Task<List<T>> MassGetObjectsRaw<T, K>(IMongoCollection<T> collec, FilterDefinition<T> query, string fieldName, List<K> targets)
        {
            //Get filter to read
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.In(fieldName, targets) & query;

            //Fetch
            var results = await collec.FindAsync(filter);
            var data = await results.ToListAsync();

            return data;
        }

        public async Task<Dictionary<ulong, SavedDinoTribePrefs>> MassGetDinoPrefs(DbServer server, List<DbDino> dinos)
        {
            //Convert dinos to a list of ulongs
            List<ulong> ids = new List<ulong>();
            foreach(var d in dinos)
            {
                if (!ids.Contains(d.dino_id))
                    ids.Add(d.dino_id);
            }
            
            //Mass fetch data
            var filterBuilder = Builders<DbSavedDinoTribePrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", server._id);
            var results = await MassGetObjectsRaw<DbSavedDinoTribePrefs, ulong>(system_saved_dino_tribe_prefs, filter, "dino_id", ids);

            //Clean up this array
            Dictionary<ulong, SavedDinoTribePrefs> output = new Dictionary<ulong, SavedDinoTribePrefs>();
            foreach(var r in results)
            {
                if (output.ContainsKey(r.dino_id))
                    continue;
                output.Add(r.dino_id, r.payload);
            }

            //Fill in the gaps by adding placeholder datas for dinos without data
            foreach(var d in dinos)
            {
                if (output.ContainsKey(d.dino_id))
                    continue;
                output.Add(d.dino_id, new SavedDinoTribePrefs());
            }

            return output;
        }

        /// <summary>
        /// Gets a new primal data package with the mods required
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        public async Task<DeltaPrimalDataPackage> GetPrimalDataPackage(string[] mods)
        {
            return new DeltaPrimalDataPackage(mods, this);
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
        /// Only set after GetStructureMetadata is called
        /// </summary>
        private List<StructureMetadata> _structureMetadatas;

        /// <summary>
        /// Only set after GetStructureMetadata is called
        /// </summary>
        private List<string> _supportedStructureMetadatas;

        /// <summary>
        /// Gets an RPC connection. This can be used to get the RPC object anytime
        /// </summary>
        /// <returns></returns>
        public List<StructureMetadata> GetStructureMetadata()
        {
            //Create a new RPC if needed
            if(_structureMetadatas == null)
            {
                _structureMetadatas = JsonConvert.DeserializeObject<List<StructureMetadata>>(File.ReadAllText(config.structure_metadata_config));
                _supportedStructureMetadatas = new List<string>();
                foreach (var s in _structureMetadatas)
                    _supportedStructureMetadatas.AddRange(s.names);
            }
            return _structureMetadatas;
        }

        public List<string> GetSupportedStructureMetadata()
        {
            GetStructureMetadata();
            return _supportedStructureMetadatas;
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
        /// Gets a user content entry by it's token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUserContent> GetUserContentByToken(string token)
        {
            var filterBuilder = Builders<DbUserContent>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var result = await system_user_uploads.FindAsync(filter);
            DbUserContent c = await result.FirstOrDefaultAsync();
            if (c == null)
                return null;
            return c;
        }

        /// <summary>
        /// Gets a user content entry by it's name
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUserContent> GetUserContentByName(string name)
        {
            var filterBuilder = Builders<DbUserContent>.Filter;
            var filter = filterBuilder.Eq("name", name);
            var result = await system_user_uploads.FindAsync(filter);
            DbUserContent c = await result.FirstOrDefaultAsync();
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
        /// Fetches saved dino/tribe data. Will never return null
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="tribeId"></param>
        /// <param name="dinoId"></param>
        /// <returns></returns>
        public async Task<SavedDinoTribePrefs> GetDinoPrefs(ObjectId serverId, int tribeId, ulong dinoId)
        {
            var filterBuilder = Builders<DbSavedDinoTribePrefs>.Filter;
            var filter = filterBuilder.Eq("server_id", serverId) & filterBuilder.Eq("dino_id", dinoId) & filterBuilder.Eq("tribe_id", tribeId);
            var results = await system_saved_dino_tribe_prefs.FindAsync(filter);
            var data = await results.FirstOrDefaultAsync();

            //Return a new object if data is null, else return this
            if (data == null)
                return new SavedDinoTribePrefs();
            else
                return data.payload;
        }

        /// <summary>
        /// Gets a collection by it's name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IMongoCollection<T> GetContentCollection<T>(string name)
        {
            return content_database.GetCollection<T>(name);
        }

        /// <summary>
        /// Gets structures
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        public async Task<List<DbStructure>> GetStructuresInRange(TileData tile, string server_id, int tribe_id, float tolerance_additive = 0)
        {
            //Make sure structures are up to date
            GetStructureMetadata();

            //Commit query
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = filterBuilder.Eq("server_id", server_id) &
                filterBuilder.Eq("tribe_id", tribe_id) &
                filterBuilder.In("classname", _supportedStructureMetadatas) &
                filterBuilder.Gt("location.x", tile.game_min_x - tolerance_additive) &
                filterBuilder.Lt("location.x", tile.game_max_x + tolerance_additive) &
                filterBuilder.Gt("location.y", tile.game_min_y - tolerance_additive) &
                filterBuilder.Lt("location.y", tile.game_max_y + tolerance_additive);
            var results = await content_structures.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Gets all tribe structures
        /// </summary>
        /// <param name="target"></param>
        /// <param name="tile"></param>
        /// <returns></returns>
        public async Task<List<DbStructure>> GetTribeStructures(DbServer server, int? tribe_id)
        {
            //Make sure structures are up to date
            GetStructureMetadata();

            //Commit query
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = FilterBuilderToolDb.CreateTribeFilter<DbStructure>(server, tribe_id) & 
                filterBuilder.In("classname", _supportedStructureMetadatas);
            var results = await content_structures.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Logs a system error
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="system"></param>
        /// <param name="version_major"></param>
        /// <param name="version_minor"></param>
        /// <param name="extras"></param>
        /// <returns></returns>
        public async Task<DbErrorLog> LogSystemError(Exception ex, Dictionary<string, string> extras)
        {
            //Create and insert
            DbErrorLog l = new DbErrorLog
            {
                extras = extras,
                is_standard = false,
                message = ex.Message,
                stack_trace = ex.StackTrace,
                system = system_name,
                system_version_major = system_version_major,
                system_version_minor = system_version_minor,
                time = DateTime.UtcNow,
                _id = ObjectId.GenerateNewId()
            };

            //Add
            await system_error_log.InsertOneAsync(l);

            return l;
        }

        /// <summary>
        /// Logs an error and creates a response to send over HTTP.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="extras"></param>
        /// <param name="message"></param>
        /// <param name="message_more"></param>
        /// <returns></returns>
        public async Task<HttpErrorResponse> LogHttpError(Exception ex, Dictionary<string, string> extras, string message = "A system error occurred.", string message_more = null)
        {
            //Log error
            DbErrorLog log = await LogSystemError(ex, extras);

            //Create response
            HttpErrorResponse response = new HttpErrorResponse
            {
                message = message,
                message_more = message_more,
                support_tag = log.id
            };

            return response;
        }

        private Dictionary<string, DbSteamCache> memory_steam_cache = new Dictionary<string, DbSteamCache>();

        /// <summary>
        /// Gets or downloads the Steam profile. The only time this will ever return null is if Steam can't find the user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbSteamCache> GetSteamProfileById(string id)
        {
            //Check if we have any in memory
            if (memory_steam_cache.ContainsKey(id))
                return memory_steam_cache[id];

            //Check if we have any (up to date) steam profiles in the database
            DbSteamCache profile = null;
            {
                //Get the latest date we can use
                long time = DateTime.UtcNow.AddMinutes(config.steam_cache_expire_minutes).Ticks;

                //Fetch
                var filterBuilder = Builders<DbSteamCache>.Filter;
                var filter = filterBuilder.Eq("steam_id", id) & filterBuilder.Lt("time_utc", time);
                var results = await system_steam_cache.FindAsync(filter);
                profile = await results.FirstOrDefaultAsync();
            }

            //If the profile was valid, return it
            if (profile != null)
                return profile;

            //We'll fetch updated Steam info
            SteamProfile_Full profiles;
            try
            {
                var response = await http.GetAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + config.steam_api_token + "&steamids=" + id);
                if (!response.IsSuccessStatusCode)
                    return null;
                profiles = JsonConvert.DeserializeObject<SteamProfile_Full>(await response.Content.ReadAsStringAsync());
            } catch
            {
                return null;
            }

            //Get the profile from this
            if (profiles.response.players.Count != 1)
                return null;
            SteamProfile profileData = profiles.response.players[0];

            //Create a profile object
            profile = new DbSteamCache
            {
                icon_url = profileData.avatarfull,
                name = profileData.personaname,
                profile_url = profileData.profileurl,
                steam_id = profileData.steamid,
                time_utc = DateTime.UtcNow.Ticks
            };

            //Now, insert for future use
            {
                var filterBuilder = Builders<DbSteamCache>.Filter;
                var filter = filterBuilder.Eq("steam_id", id);
                var response = await system_steam_cache.FindOneAndReplaceAsync<DbSteamCache>(filter, profile, new FindOneAndReplaceOptions<DbSteamCache, DbSteamCache>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });
                profile._id = response._id;
            }

            //Add to memory cache
            if (!memory_steam_cache.ContainsKey(id))
                memory_steam_cache.Add(id, profile);

            return profile;
        }

        /// <summary>
        /// Gets or downloads the Steam profile. The only time this will ever return null is if Steam can't find the user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbSteamModCache> GetSteamModById(string id)
        {
            //First, check if we have any (up to date) steam profiles
            DbSteamModCache profile = null;
            {
                //Get the latest date we can use
                long time = DateTime.UtcNow.AddMinutes(config.steam_cache_expire_minutes).Ticks;

                //Fetch
                var filterBuilder = Builders<DbSteamModCache>.Filter;
                var filter = filterBuilder.Eq("publishedfileid", id) & filterBuilder.Lt("time_utc", time);
                var results = await system_steam_mod_cache.FindAsync(filter);
                profile = await results.FirstOrDefaultAsync();
            }

            //If the profile was valid, return it
            if (profile != null)
            {
                if (profile.result != 1)
                    return null;
                return profile;
            }

            //We'll fetch updated Steam info
            SteamModDataRootObject profiles;
            try
            {
                var request = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "itemcount", "1" },
                    { "publishedfileids[0]", id }
                });
                var response = await http.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", request);
                if (!response.IsSuccessStatusCode)
                    return null;
                profiles = JsonConvert.DeserializeObject<SteamModDataRootObject>(await response.Content.ReadAsStringAsync());
            }
            catch
            {
                return null;
            }

            //Get the profile from this
            if (profiles.response.publishedfiledetails.Count != 1)
                return null;
            profile = profiles.response.publishedfiledetails[0];

            //Check profile
            if (profile.result != 1)
                return null;

            //Now, insert for future use
            {
                var filterBuilder = Builders<DbSteamModCache>.Filter;
                var filter = filterBuilder.Eq("steam_id", id);
                var response = await system_steam_mod_cache.FindOneAndReplaceAsync<DbSteamModCache>(filter, profile, new FindOneAndReplaceOptions<DbSteamModCache, DbSteamModCache>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });
                profile._id = response._id;
            }

            return profile;
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
            if(clear)
            {
                foreach(var r in resultList)
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
        /// Returns a server by it's ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbMachine> GetMachineByIdAsync(string id)
        {
            //Fetch
            DbMachine u = await GetDocumentById<DbMachine>(system_machines, id);

            //If not found, return null
            if (u == null)
                return null;

            return u;
        }

        /// <summary>
        /// Returns a machine from it's shorthand token and ensures that it is still valid.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbMachine> GetMachineByShorthandTokenAsync(string token)
        {
            //Catch cheaters and make sure that the token is not null
            if (token == null)
                return null;
            
            //Fetch
            var filterBuilder = Builders<DbMachine>.Filter;
            var filter = filterBuilder.Eq("shorthand_token", token) & filterBuilder.Gt("first_activation_time", DateTime.UtcNow.AddMinutes(-1));
            var results = await system_machines.FindAsync(filter);
            var u = await results.FirstOrDefaultAsync();

            //If not found, return null
            if (u == null)
                return null;

            //If this machine is somehow activated, do not return it
            if (u.is_activated)
                return null;

            return u;
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
        /// Returns a token object from the token string
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbToken> GetTokenByPreflightAsync(string token)
        {
            //Make sure that a token is set
            if (token == null)
                return null;
            
            //Find
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("oauth_preflight", token);
            var results = await system_tokens.FindAsync(filter);
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
        /// Authenticates a machine with a token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbMachine> AuthenticateMachineTokenAsync(string token)
        {
            //Find the machine in the database
            var filterBuilder = Builders<DbMachine>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var results = await system_machines.FindAsync(filter);
            var machine =  await results.FirstOrDefaultAsync();
            return machine;
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
        /// Authenticates a server with it's token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbServer> AuthenticateServerMirrorTokenAsync(string token)
        {
            //Find the machine in the database
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("mirror_token", token);
            var results = await system_servers.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
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

        /// <summary>
        /// Returns all player profiles for a server
        /// </summary>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public async Task<List<DbPlayerProfile>> GetServerPlayerProfilesAsync(string serverId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", serverId);
            var results = await content_player_profiles.FindAsync(filter);
            return await results.ToListAsync();
        }


        /// <summary>
        /// Returns all player profiles in a tribe
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public async Task<List<DbPlayerProfile>> GetServerPlayerProfilesByTribeAsync(string serverId, int tribeId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("server_id", serverId) & filterBuilder.Eq("tribe_id", tribeId);
            var results = await content_player_profiles.FindAsync(filter);
            return await results.ToListAsync();
        }

        public async Task<List<DbUser>> GetAllUsersInTribe(string serverId, int tribeId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("server_id", serverId);
            var results = await content_player_profiles.FindAsync(filter);
            throw new NotImplementedException(); //TODO!!!
        }

        /// <summary>
        /// Returns a cached dynamic tile, if any
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public async Task<DbDynamicTileCache> GetCachedDynamicTile(DynamicTileTarget target)
        {
            var results = await system_dynamic_tile_cache.FindAsync(target.CreateFilter());
            return await results.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Validates if a key can be claimed. If claimer is set, it will be claimed by that user ID
        /// </summary>
        /// <param name="claimer"></param>
        /// <returns></returns>
        public async Task<bool> ValidateAndClaimBetaKey(string key, ObjectId? claimer = null)
        {
            //Search for key
            var filterBuilder = Builders<DbBetaKey>.Filter;
            var filter = filterBuilder.Eq("key", key);
            var result = await (await system_beta_keys.FindAsync(filter)).FirstOrDefaultAsync();
            if (result == null)
                return false;
            if(claimer == null)
            {
                //No claimer set. We don't know if this belongs to us, so return true
                return true;
            } else
            {
                //A claimer is set. Make sure that this is either unclaimed or claimed by the same user
                if (claimer.Value == result.claimed_by || !result.claimed)
                {
                    //Success. Claim
                    var update = Builders<DbBetaKey>.Update.Set("claimed", true).Set("claimed_by", claimer.Value);
                    await system_beta_keys.UpdateOneAsync(filter, update);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Creates a new beta key
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateBetaKey(string note)
        {
            //Generate key
            char[] key = SecureStringTool.GenerateSecureString(14).ToCharArray();
            key[4] = '-';
            key[9] = '-';

            //Add key
            await system_beta_keys.InsertOneAsync(new DbBetaKey
            {
                claimed = false,
                key = new string(key),
                note = note,
            });

            return new string(key);
        }
    }
}
