using LibDeltaSystem.CoreNet;
using LibDeltaSystem.CoreNet.NetMessages;
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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LibDeltaSystem
{
    public class DeltaConnection : DeltaDatabaseConnection, IDeltaLogger
    {
        public const byte LIB_VERSION_MAJOR = 1;
        public const byte LIB_VERSION_MINOR = 5;

        public RouterConnection net;
        public string instanceId;
        public HttpClient http;
        public DeltaEventMaster events;

        public DeltaCoreNetServerType server_type;
        public byte system_version_minor;
        public byte system_version_major;

        public LoginServerConfigHosts hosts;
        public bool loggingEnabled = true;
        public string steamApiKey;
        public int steamCacheExpireMinutes;
        public byte[] steamTokenKey;
        public int[] userPorts;
        public string firebaseUcBucket;
        public string enviornment;

        public DeltaConnection(int router_port, long router_key, byte system_version_major, byte system_version_minor, DeltaCoreNetServerType server_type)
        {
            //Set vars
            this.http = new HttpClient();
            this.system_version_major = system_version_major;
            this.system_version_minor = system_version_minor;
            this.server_type = server_type;

            //Open network if it isn't disabled
            if(router_port != -1)
                net = new RouterConnection(new IPEndPoint(IPAddress.Loopback, router_port), router_key, this);
        }

        /// <summary>
        /// Initializes the session online, connected to a router server
        /// </summary>
        /// <returns></returns>
        public async Task InitNetworked()
        {
            //Wait for login to succeed.
            Log("Init", "Login packet sent, waiting for router to respond...", DeltaLogLevel.Medium);
            while (!net.loggedIn) ;

            //Request config
            Log("Init", "Login succeeded. Requesting config, waiting for router to respond...", DeltaLogLevel.Medium);
            var loginDetails = await net.RequestConfig();

            //Make sure login was successful
            if (!loginDetails.success)
                throw new Exception("Failed to request config. Server rejected our request.");

            //Set config
            enviornment = loginDetails.config.enviornment;
            loggingEnabled = loginDetails.config.log;
            steamApiKey = loginDetails.config.steam_api_key;
            steamCacheExpireMinutes = loginDetails.config.steam_cache_expire_minutes;
            firebaseUcBucket = loginDetails.config.firebase_uc_bucket;
            steamTokenKey = Convert.FromBase64String(loginDetails.config.steam_token_key);
            userPorts = loginDetails.user_ports;
            instanceId = loginDetails.instance_id;
            hosts = loginDetails.config.hosts;

            //Connect to database
            OpenDatabase(loginDetails.config.mongodb_connection, loginDetails.config.enviornment);

            //Open event handler
            events = new DeltaEventMaster(this);

            //Log
            Log("Init", $"Init succeeded. Connected with instance ID {loginDetails.instance_id}.", DeltaLogLevel.Medium);
        }

        /// <summary>
        /// Initializes the session offline, or not connected to any router server
        /// </summary>
        /// <returns></returns>
        public void InitOffline(LoginServerConfig config, int[] ports)
        {
            //Set config
            enviornment = config.enviornment;
            loggingEnabled = config.log;
            steamApiKey = config.steam_api_key;
            steamCacheExpireMinutes = config.steam_cache_expire_minutes;
            firebaseUcBucket = config.firebase_uc_bucket;
            steamTokenKey = Convert.FromBase64String(config.steam_token_key);
            userPorts = ports;
            instanceId = "OFFLINE_INSTANCE";
            hosts = config.hosts;

            //Connect to database
            OpenDatabase(config.mongodb_connection, config.enviornment);

            //Log
            Log("Init", $"Init succeeded. Running offline.", DeltaLogLevel.Medium);
        }

        /// <summary>
        /// Creates a managed app. StartupArgs are from the args used to start the application. Works with args passed by this bring run by a proccess manager. Connects too
        /// </summary>
        /// <param name="startupArgs"></param>
        /// <param name="system_version_major"></param>
        /// <param name="system_version_minor"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        public static DeltaConnection InitDeltaManagedApp(string[] startupArgs, DeltaCoreNetServerType server_type, byte system_version_major, byte system_version_minor)
        {
            //Validate
            if (startupArgs.Length != 2)
                throw new Exception("The startup args are not valid. This program is supposed to be run from a Delta Process Manager.");

            //Log
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Starting Delta managed app on version {system_version_major}.{system_version_minor} (lib version {LIB_VERSION_MAJOR}.{LIB_VERSION_MINOR})...");
            Console.ForegroundColor = ConsoleColor.White;

            //Create
            var d = new DeltaConnection(int.Parse(startupArgs[0]), long.Parse(startupArgs[1]), system_version_major, system_version_minor, server_type);

            //Connect
            d.InitNetworked().GetAwaiter().GetResult();

            return d;
        }

        /// <summary>
        /// Returns a port specified to this application
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetUserPort(int index)
        {
            return userPorts[index];
        }

        /// <summary>
        /// Loads a user config if it exists. If it doesn't, the default is returned and also written to disk
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public async Task<T> GetUserConfig<T>(string name, T defaultValue)
        {
            //Serialize default value to send
            string defaultValueSer = JsonConvert.SerializeObject(defaultValue, Formatting.Indented);

            //Get
            string cfg = await GetUserConfigString(name, defaultValueSer);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(cfg);
        }

        public Task<string> GetUserConfigString(string name, string defaultValue)
        {
            return net.SendLoadUserConfigCommand(name, defaultValue);
        }

        public const string CONFIGNAME_STRUCTURE_METADATA = "structure_metadata.json";
        public const string CONFIGNAME_FIREBASE = "firebase_config.json";

        public void Log(string topic, string message, DeltaLogLevel level)
        {
            //Log to stdout
            if(loggingEnabled)
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
            if(net != null && (loggingEnabled || (int)level >= (int)DeltaLogLevel.High) && !topic.StartsWith("Init") && !topic.StartsWith("RouterIO"))
                net.SendLogCommand(topic, message, level);
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
        public async Task<List<StructureMetadata>> GetStructureMetadata()
        {
            //Create a new RPC if needed
            if(_structureMetadatas == null)
            {
                _structureMetadatas = await GetUserConfig<List<StructureMetadata>>(CONFIGNAME_STRUCTURE_METADATA, new List<StructureMetadata>());
                _supportedStructureMetadatas = new List<string>();
                foreach (var s in _structureMetadatas)
                    _supportedStructureMetadatas.AddRange(s.names);
            }
            return _structureMetadatas;
        }

        private Dictionary<string, DbSteamCache> memory_steam_cache = new Dictionary<string, DbSteamCache>();

        public async Task<Dictionary<string, DbSteamCache>> BulkGetSteamProfiles(List<string> ids)
        {
            //Make sure there aren't too many
            if (ids.Count > 50)
                throw new Exception("A maxiumum of 50 profiles is allowed.");

            //Create a dict for this
            Dictionary<string, DbSteamCache> profiles = new Dictionary<string, DbSteamCache>();

            //Search for items in cache
            List<DbSteamCache> cacheHits;
            {
                var filterBuilder = Builders<DbSteamCache>.Filter;
                var filter = filterBuilder.In("steam_id", ids) & filterBuilder.Lt("time_utc", DateTime.UtcNow.AddMinutes(steamCacheExpireMinutes).Ticks);
                cacheHits = await (await system_steam_cache.FindAsync(filter)).ToListAsync();
            }
            foreach(var r in cacheHits)
            {
                if(!profiles.ContainsKey(r.steam_id))
                    profiles.Add(r.steam_id, r);
            }

            //Check if we've found all already
            if (profiles.Count == ids.Count)
                return profiles;

            //Build the Steam request url
            string steamRequestUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + steamApiKey + "&steamids=";
            int steamRequestCount = 0;
            foreach(string s in ids)
            {
                //Skip if this was hit in the cache
                if (profiles.ContainsKey(s))
                    continue;

                //Add comma if needed
                if (steamRequestCount != 0)
                    steamRequestUrl += ",";

                //Add
                steamRequestCount++;
                steamRequestUrl += HttpUtility.UrlEncode(s);
            }

            //Hit Steam if we did not find all
            if(steamRequestCount > 0)
            {
                //We'll fetch updated Steam info
                SteamProfile_Full steamHits;
                try
                {
                    var response = await http.GetAsync(steamRequestUrl);
                    if (!response.IsSuccessStatusCode)
                        throw new Exception();
                    steamHits = JsonConvert.DeserializeObject<SteamProfile_Full>(await response.Content.ReadAsStringAsync());
                }
                catch
                {
                    return profiles;
                }

                //Add each hit
                foreach (var profileData in steamHits.response.players)
                {
                    //Create a profile object
                    var profile = new DbSteamCache
                    {
                        icon_url = profileData.avatarfull,
                        name = profileData.personaname,
                        profile_url = profileData.profileurl,
                        steam_id = profileData.steamid,
                        time_utc = DateTime.UtcNow.Ticks
                    };

                    //Insert into database cache for future use
                    {
                        var filterBuilder = Builders<DbSteamCache>.Filter;
                        var filter = filterBuilder.Eq("steam_id", profileData.steamid);
                        var response = await system_steam_cache.FindOneAndReplaceAsync<DbSteamCache>(filter, profile, new FindOneAndReplaceOptions<DbSteamCache, DbSteamCache>
                        {
                            IsUpsert = true,
                            ReturnDocument = ReturnDocument.After
                        });
                        profile._id = response._id;
                    }

                    //Add
                    if (!profiles.ContainsKey(profile.steam_id))
                        profiles.Add(profile.steam_id, profile);
                }
            }

            return profiles;
        }

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
                long time = DateTime.UtcNow.AddMinutes(steamCacheExpireMinutes).Ticks;

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
                var response = await http.GetAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + steamApiKey + "&steamids=" + id);
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
                long time = DateTime.UtcNow.AddMinutes(steamCacheExpireMinutes).Ticks;

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

        /// <summary>
        /// Produces a token for a specific Steam ID. This token is signed by this server to prevent abuse. It can be checked later by any DeltaWebMap server.
        /// </summary>
        /// <returns></returns>
        public string CreateSteamIdTokenString(string steamId)
        {
            //Create a buffer for the message
            byte[] buffer = new byte[16 + 1 + Encoding.ASCII.GetByteCount(steamId)];

            //Write buffer contents
            steamTokenKey.CopyTo(buffer, 0);
            buffer[16] = 0x01;
            Encoding.ASCII.GetBytes(steamId, 0, steamId.Length, buffer, 17);

            //Create HMAC and write it
            byte[] hash = new HMACMD5(steamTokenKey).ComputeHash(buffer);
            if (hash.Length != 16)
                throw new Exception("Invalid hash length.");
            hash.CopyTo(buffer, 0);

            //Return the BASE64 version of this
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Reads the data encoded by the CreateSteamIdTokenString function
        /// </summary>
        /// <param name="encoded"></param>
        public SteamIdToken ReadSteamIdTokenString(string encoded)
        {
            //Read as bytes
            byte[] data;
            try
            {
                data = Convert.FromBase64String(encoded);
            } catch
            {
                return null;
            }

            //Validate
            if (data.Length < 16 + 1)
                return null;

            //Read the hash
            byte[] hash = new byte[16];
            Array.Copy(data, 0, hash, 0, 16);

            //Get the HMAC of this
            steamTokenKey.CopyTo(data, 0);
            byte[] challengeHash = new HMACMD5(steamTokenKey).ComputeHash(data);

            //Validate
            if (!BinaryTool.CompareBytes(hash, challengeHash))
                return null;

            //Data OK! Read it now
            return new SteamIdToken
            {
                version = data[16],
                steam_id = Encoding.ASCII.GetString(data, 17, data.Length - 17)
            };
        }
    }
}
