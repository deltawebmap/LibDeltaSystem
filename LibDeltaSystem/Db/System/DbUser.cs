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
            string token = DbToken.GenerateSecureString(64);
            while(conn.AuthenticateUserToken(token).GetAwaiter().GetResult() != null)
                token = DbToken.GenerateSecureString(64);

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
        /// Finds and devalidates all tokens belonging to this user.
        /// </summary>
        /// <returns></returns>
        public async Task<int> DevalidateAllTokens()
        {
            //Search for all
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("user_id", id);
            var results = await conn.system_tokens.FindAsync(filter);
            var resultList = await results.ToListAsync();

            //Clear all
            foreach (var r in resultList)
                r.DeleteAsync().GetAwaiter().GetResult();

            return resultList.Count;
        }
    }
}
