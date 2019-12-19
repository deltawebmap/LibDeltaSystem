using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbToken : DbBaseSystem
    {
        /// <summary>
        /// The token string
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The user ID this maps to
        /// </summary>
        public string user_id { get; set; }

        /// <summary>
        /// The time this token was created
        /// </summary>
        public long created_utc { get; set; }

        /// <summary>
        /// If false, this is a system issued token
        /// </summary>
        public bool is_oauth { get; set; }

        /// <summary>
        /// The OAUTH client ID used to register this user, if is_oauth is set
        /// </summary>
        public string oauth_client_id { get; set; }

        /// <summary>
        /// The OAUTH scopes requested by this application. Ignored if is_oauth == false
        /// </summary>
        public string[] oauth_scopes { get; set; }

        /// <summary>
        /// "Preflight" token sent to the 3rd party's backend server and used to obtain a real token
        /// </summary>
        public string oauth_preflight { get; set; }

        public const string SCOPE_USER_INFO = "USER_INFO";
        public const string SCOPE_VIEW_SERVER_INFO = "VIEW_SERVER_INFO";
        public const string SCOPE_PUT_DINO_PREFS = "PUT_DINO_PREFS";

        /// <summary>
        /// Checks if this token is authorized to send a request
        /// </summary>
        /// <param name="scope">Name</param>
        /// <returns></returns>
        public bool CheckScope(string scope)
        {
            //If this is a system token, this is always permitted
            if (!is_oauth)
                return true;

            //If scope is null, do not allow oauth tokens
            if (scope == null)
                return false;

            //Check if within scope bounds
            return oauth_scopes.Contains(scope);
        }

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
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_tokens.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_tokens.FindOneAndDeleteAsync(filter);
        }
    }
}
