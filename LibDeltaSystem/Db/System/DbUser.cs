﻿using LibDeltaSystem.Db.Content;
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
        /// Token used for server creation
        /// </summary>
        public string server_creation_token { get; set; }

        /// <summary>
        /// Contains all alert banners that have been dismissed
        /// </summary>
        public List<ObjectId> dismissed_alert_banners { get; set; } = new List<ObjectId>();

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update(DeltaConnection conn)
        {
            UpdateAsync(conn).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates the server creation token
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetServerCreationToken(DeltaConnection conn)
        {
            if (server_creation_token == null)
            {
                server_creation_token = Tools.SecureStringTool.GenerateSecureString(44);
                await UpdateAsync(conn);
            }
            return server_creation_token;
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_users.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbUser>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_users.FindOneAndDeleteAsync(filter);
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
        public async Task<List<Tuple<DbServer, DbPlayerProfile>>> GetGameServersAsync(DeltaConnection conn)
        {
            //Search for player profiles
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = filterBuilder.Eq("steam_id", steam_id);
            var profiles = await conn.content_player_profiles.FindAsync(filter);
            var profilesList = await profiles.ToListAsync();

            //Now, find all servers
            List<ObjectId> serverIds = new List<ObjectId>();
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

        public async Task<List<DbServer>> GetAdminedServersAsync(DeltaConnection conn)
        {
            //Search for servers
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("owner_uid", this._id) | filterBuilder.In("admins", new ObjectId[]{this._id});
            var result = await conn.system_servers.FindAsync(filter);
            return await result.ToListAsync();
        }

        /// <summary>
        /// Generates a random token
        /// </summary>
        /// <returns></returns>
        public async Task<string> MakeOauthToken(DeltaConnection conn, ObjectId oauthApp, uint oauthScope)
        {
            //Generate a unique string
            string token = Tools.SecureStringTool.GenerateSecureString(62);
            while(await conn.AuthenticateUserToken(token) != null)
                token = Tools.SecureStringTool.GenerateSecureString(62);

            //Add some information. This is for future use
            token = "A." + token;

            //Now, create a token object
            DbToken t = new DbToken
            {
                created_utc = DateTime.UtcNow,
                token = token,
                user_id = _id,
                _id = MongoDB.Bson.ObjectId.GenerateNewId(),
                oauth_application = oauthApp,
                oauth_scope = oauthScope
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
        public async Task<long> DevalidateAllTokens(DeltaConnection conn)
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
                    _id = MongoDB.Bson.ObjectId.GenerateNewId()
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
        public static async Task<DbUser> AuthenticateUserToken(DeltaDatabaseConnection conn, string token)
        {
            //First, get our token object
            DbToken tok = await conn.GetTokenByTokenAsync(token);
            if (tok == null)
                return null;

            //Now, get our user
            var u = await conn.GetUserByIdAsync(tok.user_id);
            if (u == null)
                return null;
            return u;
        }
    }
}
