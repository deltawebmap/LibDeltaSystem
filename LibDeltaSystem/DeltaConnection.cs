﻿using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.PrivateNet;
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
        private MongoClient content_client;
        private IMongoDatabase content_database;
        private IMongoDatabase system_database;

        public IMongoCollection<DbDino> content_dinos;
        public IMongoCollection<DbItem> content_items;
        public IMongoCollection<DbTribe> content_tribes;
        public IMongoCollection<DbPlayerProfile> content_player_profiles;
        public IMongoCollection<DbStructure> content_structures;
        public IMongoCollection<DbTribeLogEntry> content_tribe_log;

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

        public DeltaConnectionConfig config;

        public HttpClient http;

        public string system_name;
        public int system_version_minor;
        public int system_version_major;

        public DeltaConnection(string pathname, string system_name, int system_version_major, int system_version_minor)
        {
            config = JsonConvert.DeserializeObject<DeltaConnectionConfig>(File.ReadAllText(pathname));
            this.http = new HttpClient();
            this.system_version_major = system_version_major;
            this.system_version_minor = system_version_minor;
            this.system_name = system_name;
        }

        public DeltaConnection(DeltaConnectionConfig config, string system_name, int system_version_major, int system_version_minor)
        {
            this.config = config;
            this.http = new HttpClient();
            this.system_version_major = system_version_major;
            this.system_version_minor = system_version_minor;
            this.system_name = system_name;
        }

        private static async Task<T> GetDocumentById<T>(IMongoCollection<T> collec, string id)
        {
            //Find
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("_id", ObjectId.Parse(id));
            var results = await collec.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            return r;
        }

        /// <summary>
        /// Connects and sets up databases
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            //Set up database
            content_client = new MongoClient(
                $"mongodb://{config.user}:{config.key}@{config.server_ip}:{config.server_port}"
            );

            content_database = content_client.GetDatabase("delta-"+config.env);
            content_dinos = content_database.GetCollection<DbDino>("dinos");
            content_items = content_database.GetCollection<DbItem>("items");
            content_tribes = content_database.GetCollection<DbTribe>("tribes");
            content_player_profiles = content_database.GetCollection<DbPlayerProfile>("player_profiles");
            content_structures = content_database.GetCollection<DbStructure>("structures");
            content_tribe_log = content_database.GetCollection<DbTribeLogEntry>("tribe_log_entries");

            system_database = content_client.GetDatabase("delta-system-"+config.env);
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

            //Set up Google Firebase
            if(config.firebase_config != null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(config.firebase_config),
                });
            }
        }

        /// <summary>
        /// Sends a Firebase notification to all users with this tribe. Returns number of successful notifications.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="tribe_id"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<int> SendPushNotificationToTribe(DbServer server, int tribe_id, string body)
        {
            //Find all tokens to use
            List<string> registrationTokens = new List<string>();
            var users = await GetAllUsersInTribe(server.id, tribe_id);
            foreach(var u in users)
            {
                if (u.notification_tokens != null)
                    registrationTokens.AddRange(u.notification_tokens);
            }

            //Create message payload
            var message = new MulticastMessage()
            {
                Tokens = registrationTokens,
                Notification = new Notification
                {
                    ImageUrl = server.image_url,
                    Title = server.display_name,
                    Body = body
                }
            };

            //Send
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            //Return count
            return response.SuccessCount;
        }

        /// <summary>
        /// Fetches saved dino/tribe data. Will never return null
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="tribeId"></param>
        /// <param name="dinoId"></param>
        /// <returns></returns>
        public async Task<SavedDinoTribePrefs> GetDinoPrefs(string serverId, int tribeId, ulong dinoId)
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

        /// <summary>
        /// Gets or downloads the Steam profile. The only time this will ever return null is if Steam can't find the user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbSteamCache> GetSteamProfileById(string id)
        {
            //First, check if we have any (up to date) steam profiles
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
                time_utc = DateTime.UtcNow.Ticks,
                conn = this
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
                return profile;

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
            profile.conn = this;

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

            //Add some props
            u.conn = this;

            return u;
        }

        /// <summary>
        /// Returns a user by their user ID. Returns null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DbServer> GetServerByIdAsync(string id)
        {
            //Fetch
            DbServer u = await GetDocumentById<DbServer>(system_servers, id);

            //If not found, return null
            if (u == null)
                return null;

            //Add some props
            u.conn = this;

            return u;
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

            //Add some props
            u.conn = this;

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

            //Set conn
            r.conn = this;

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

            //Set conn
            r.conn = this;

            return r;
        }

        /// <summary>
        /// Authenticates a user with a token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<DbUser> AuthenticateUserToken(string token)
        {
            //First, get our token object
            DbToken tok = await GetTokenByTokenAsync(token);
            if (tok == null)
                return null;

            //Now, get our user
            return await GetUserByIdAsync(tok.user_id);
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
            machine.conn = this;
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
            return await results.FirstOrDefaultAsync();
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
        public async Task<DbTribe> GetTribeByTribeIdAsync(string serverId, int tribeId)
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

        public async Task<List<DbUser>> GetAllUsersInTribe(string serverId, int tribeId)
        {
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("server_id", serverId);
            var results = await content_player_profiles.FindAsync(filter);
            throw new NotImplementedException(); //TODO!!!
        }
    }
}
