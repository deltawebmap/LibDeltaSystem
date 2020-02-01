using LibDeltaSystem.Db.Content;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbUser : DbBaseSystem
    {
        /// <summary>
        /// The Steam ID. Used for authentication
        /// </summary>
        public string steam_id { get; set; }

        /// <summary>
        /// URL to the Steam profile
        /// </summary>
        public string steam_profile_url { get; set; }

        /// <summary>
        /// The profile picture URL
        /// </summary>
        public string profile_image_url { get; set; }

        /// <summary>
        /// The name this user goes by
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// The user settings for this user
        /// </summary>
        public DbUserSettings user_settings { get; set; }

        /// <summary>
        /// Tokens for sending Firebase notifications
        /// </summary>
        public List<string> notification_tokens { get; set; }

        /// <summary>
        /// Token used for server creation
        /// </summary>
        public string server_creation_token { get; set; }

        /// <summary>
        /// Token that was used to authenticate this user
        /// </summary>
        [BsonIgnore]
        private DbToken _token { get; set; }

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the token used to authenticate this user
        /// </summary>
        /// <returns></returns>
        public DbToken GetAuthenticatedToken()
        {
            return _token;
        }

        /// <summary>
        /// Generates the server creation token
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetServerCreationToken()
        {
            if (server_creation_token == null)
            {
                server_creation_token = Tools.SecureStringTool.GenerateSecureString(44);
                await UpdateAsync();
            }
            return server_creation_token;
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync()
        {
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_users.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_users.FindOneAndDeleteAsync(filter);
        }

        /// <summary>
        /// Returns servers this member is part of. Does not include servers that they own but do not play on.
        /// </summary>
        /// <returns></returns>
        public List<Tuple<DbServer, DbPlayerProfile>> GetGameServers()
        {
            return GetGameServersAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if this user is a premium, subscribed, member
        /// </summary>
        /// <returns></returns>
        public bool GetIsPremium()
        {
            //This'll be added in the future sometime...
            return true;
        }

        /// <summary>
        /// Returns servers this member is part of. Does not include servers that they own but do not play on.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tuple<DbServer, DbPlayerProfile>>> GetGameServersAsync()
        {
            //Search for player profiles
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("steam_id", steam_id);
            var profiles = await conn.content_player_profiles.FindAsync(filter);
            var profilesList = await profiles.ToListAsync();

            //Now, find all servers
            List<string> serverIds = new List<string>();
            List<Tuple<DbServer, DbPlayerProfile>> servers = new List<Tuple<DbServer, DbPlayerProfile>>();
            foreach(var p in profilesList)
            {
                //Skip if we already have this server Id
                if (serverIds.Contains(p.server_id))
                    continue;
                
                //Get this server by it's ID
                var server = await conn.GetServerByIdAsync(p.server_id);
                serverIds.Add(p.server_id);

                //Add
                servers.Add(new Tuple<DbServer, DbPlayerProfile>(server, p));
            }

            return servers;
        }

        /// <summary>
        /// Returns servers that this user owns, but might not actually play on.
        /// </summary>
        /// <returns></returns>
        public async Task<List<DbServer>> GetOwnedServersAsync()
        {
            //Search for servers we own
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("owner_uid", id);
            var results = await conn.system_servers.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Generates a random token
        /// </summary>
        /// <returns></returns>
        public async Task<string> MakeToken()
        {
            //Generate a unique string
            string token = Tools.SecureStringTool.GenerateSecureString(64);
            while(await conn.AuthenticateUserToken(token) != null)
                token = Tools.SecureStringTool.GenerateSecureString(64);

            //Now, create a token object
            DbToken t = new DbToken
            {
                created_utc = DateTime.UtcNow.Ticks,
                token = token,
                user_id = id,
                _id = MongoDB.Bson.ObjectId.GenerateNewId()
            };

            //Insert
            await conn.system_tokens.InsertOneAsync(t);

            //Return string
            return token;
        }

        /// <summary>
        /// Generates a random token
        /// </summary>
        /// <returns></returns>
        public async Task<DbToken> MakeOAuthToken(DbOauthApp app, string[] scopes)
        {
            //Generate a unique string
            string token = Tools.SecureStringTool.GenerateSecureString(64);
            while (await conn.AuthenticateUserToken(token) != null)
                token = Tools.SecureStringTool.GenerateSecureString(64);

            //Generate a unique prefight token
            string preflight = Tools.SecureStringTool.GenerateSecureString(64);
            while (await conn.GetTokenByPreflightAsync(preflight) != null)
                preflight = Tools.SecureStringTool.GenerateSecureString(64);

            //Now, create a token object
            DbToken t = new DbToken
            {
                created_utc = DateTime.UtcNow.Ticks,
                token = token,
                user_id = id,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                is_oauth = true,
                oauth_client_id = app.client_id,
                oauth_preflight = preflight,
                oauth_scopes = scopes
            };

            //Insert
            await conn.system_tokens.InsertOneAsync(t);

            //Return string
            return t;
        }

        /// <summary>
        /// Finds and devalidates all tokens belonging to this user.
        /// </summary>
        /// <returns></returns>
        public async Task<long> DevalidateAllTokens()
        {
            //Search for all
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("user_id", id);
            var results = await conn.system_tokens.DeleteManyAsync(filter);

            return results.DeletedCount;
        }

        /// <summary>
        /// Gets a user by their Steam ID. If the user doesn't exist, one is created
        /// </summary>
        /// <returns></returns>
        public static async Task<DbUser> GetUserBySteamID(DeltaConnection conn, DbSteamCache profile)
        {
            //Get user. If a user account isn't created yet, make one.
            DbUser user = await conn.GetUserBySteamIdAsync(profile.steam_id);
            if (user == null)
            {
                //Create the user
                user = new DbUser
                {
                    user_settings = new DbUserSettings(),
                    profile_image_url = profile.icon_url,
                    steam_profile_url = profile.profile_url,
                    screen_name = profile.name,
                    steam_id = profile.steam_id,
                    _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                    conn = conn
                };

                //Insert in the database
                await conn.system_users.InsertOneAsync(user);
            }
            return user;
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        /// <returns></returns>
        public static async Task<DbUser> GetUserByID(DeltaConnection conn, ObjectId id)
        {
            //Get user. If a user account isn't created yet, make one.
            DbUser user = await conn.GetUserByIdAsync(id.ToString());
            return user;
        }

        /// <summary>
        /// Authenticates a user using their token
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<DbUser> AuthenticateUserToken(DeltaConnection conn, string token)
        {
            //First, get our token object
            DbToken tok = await conn.GetTokenByTokenAsync(token);
            if (tok == null)
                return null;

            //Now, get our user
            var u = await conn.GetUserByIdAsync(tok.user_id);
            if (u == null)
                return null;
            u._token = tok;
            return u;
        }
    }
}
