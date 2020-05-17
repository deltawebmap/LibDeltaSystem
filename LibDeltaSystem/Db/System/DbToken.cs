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
        /// The type of token this is
        /// </summary>
        public DbToken_TokenType token_type { get; set; }

        /// <summary>
        /// The OAUTH client ID used to register this user, if is_oauth is set
        /// </summary>
        public string oauth_client_id { get; set; }

        /// <summary>
        /// Scope flags set on this token
        /// </summary>
        public ulong token_scope { get; set; }

        /// <summary>
        /// "Preflight" token sent to the 3rd party's backend server and used to obtain a real token
        /// </summary>
        public string oauth_preflight { get; set; }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_tokens.FindOneAndDeleteAsync(filter);
        }

        public async Task ActivateOauthToken(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            var updateBuilder = Builders<DbToken>.Update;
            var update = updateBuilder.Set<string>("oauth_preflight", null);
            await conn.system_tokens.UpdateOneAsync(filter, update);
        }
    }

    public enum DbToken_TokenType
    {
        UserSystem, //This is a standard Delta Web Map token
        UserOauth //This was created from an oauth application
    }
}
