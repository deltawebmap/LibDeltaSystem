using LibDeltaSystem.CoreHub;
using LibDeltaSystem.CoreHub.CoreNetwork;
using LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
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
    public class DeltaConnection : DeltaDatabaseConnection
    {
        public const byte LIB_VERSION_MAJOR = 0;
        public const byte LIB_VERSION_MINOR = 20;

        public const int CONFIG_VERSION_LATEST = 2;

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

        public DeltaConnection(string pathname, ushort server_id, byte system_version_major, byte system_version_minor, BaseClientCoreNetwork network)
        {
            if (!File.Exists(pathname))
                throw new Exception("Delta config file was not found! Requested location: "+pathname);
            config = JsonConvert.DeserializeObject<DeltaConnectionConfig>(File.ReadAllText(pathname));
            if (config.version < CONFIG_VERSION_LATEST)
                throw new Exception("Config is out of date! Please update it's formatting and version.");
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
            //Validate
            if (startupArgs.Length != 2)
                throw new Exception("The startup args are not valid. This program is supposed to be run from a Delta Process Manager.");

            //Log
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Starting Delta managed app on version {system_version_major}.{system_version_minor} (lib version {LIB_VERSION_MAJOR}.{LIB_VERSION_MINOR})...");
            Console.WriteLine($"Using config at {startupArgs[0]} with server ID {startupArgs[1]}.");
            Console.ForegroundColor = ConsoleColor.White;

            //Create
            var d = new DeltaConnection(startupArgs[0], ushort.Parse(startupArgs[1]), system_version_major, system_version_minor, network);

            //Connect
            d.Connect().GetAwaiter().GetResult();

            return d;
        }

        /// <summary>
        /// Returns a port specified to this application
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetUserPort(int index)
        {
            return me.ports[index];
        }

        /// <summary>
        /// Loads a config file with the specified name from the config path 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetUserConfig<T>(string name)
        {
            //Read text
            string content = File.ReadAllText(GetUserConfigPath(name));

            //Deserialize
            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Loads a user config if it exists. If it doesn't, the default is returned and also written to disk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetUserConfigDefault<T>(string name, T defaultValue)
        {
            //Get the location
            string path = GetUserConfigPath(name);

            //Check if it exists
            if (File.Exists(path))
                return GetUserConfig<T>(name);

            //Write default to the disk
            Log("GetUserConfigDefault", $"Requested config file \"{name}\", but it didn't exist. One has been created on the disk.", DeltaLogLevel.High);
            File.WriteAllText(path, JsonConvert.SerializeObject(defaultValue, Formatting.Indented));

            //Return default
            return defaultValue;
        }

        /// <summary>
        /// Returns a pathname to a user config
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetUserConfigPath(string name)
        {
            return config.configs_location + name;
        }

        public const string CONFIGNAME_STRUCTURE_METADATA = "structure_metadata.json";
        public const string CONFIGNAME_FIREBASE = "firebase_config.json";

        /// <summary>
        /// Connects and sets up databases
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            //Set up database
            OpenDatabase(config.mongodb_connection, config.env);

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
            //Log to stdout
            if(config.log)
            {
                //Translate level to console color
                switch (level)
                {
                    case DeltaLogLevel.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
                    case DeltaLogLevel.Low: Console.ForegroundColor = ConsoleColor.White; break;
                    case DeltaLogLevel.Medium: Console.ForegroundColor = ConsoleColor.Yellow; break;
                    case DeltaLogLevel.High: Console.ForegroundColor = ConsoleColor.Red; break;
                    case DeltaLogLevel.Alert: Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Red; break;
                }

                //Write
                Console.WriteLine($"[{topic}] {message}");

                //Reset
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
            }

            //Remote log
            if(config.log || (int)level >= (int)DeltaLogLevel.High)
                network.RemoteLog(topic, message, level);
        }

        public void AttachWebServer(DeltaWebServer server)
        {
            this.web_server = server;
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
                _structureMetadatas = GetUserConfig<List<StructureMetadata>>(CONFIGNAME_STRUCTURE_METADATA);
                _supportedStructureMetadatas = new List<string>();
                foreach (var s in _structureMetadatas)
                    _supportedStructureMetadatas.AddRange(s.names);
            }
            return _structureMetadatas;
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
    }
}
