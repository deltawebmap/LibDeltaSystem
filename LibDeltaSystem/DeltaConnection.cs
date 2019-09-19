using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.PrivateNet;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public IMongoCollection<DbUser> system_users;
        public IMongoCollection<DbToken> system_tokens;
        public IMongoCollection<DbServer> system_servers;
        public IMongoCollection<DbSteamCache> system_steam_cache;

        public DeltaConnectionConfig config;

        public HttpClient http;

        public DeltaConnection(DeltaConnectionConfig config)
        {
            this.config = config;
            this.http = new HttpClient();
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
            content_client = new MongoClient(
                $"mongodb://{config.server_ip}:{config.server_port}"
            );

            content_database = content_client.GetDatabase("delta-"+config.env);
            content_dinos = content_database.GetCollection<DbDino>("dinos");
            content_items = content_database.GetCollection<DbItem>("items");
            content_tribes = content_database.GetCollection<DbTribe>("tribes");
            content_player_profiles = content_database.GetCollection<DbPlayerProfile>("player_profiles");

            system_database = content_client.GetDatabase("delta-system-"+config.env);
            system_users = system_database.GetCollection<DbUser>("users");
            system_tokens = system_database.GetCollection<DbToken>("tokens");
            system_servers = system_database.GetCollection<DbServer>("servers");
            system_steam_cache = system_database.GetCollection<DbSteamCache>("steam_cache");
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
