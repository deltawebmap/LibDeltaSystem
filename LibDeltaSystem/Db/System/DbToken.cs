using MongoDB.Bson;
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
        public ObjectId user_id { get; set; }

        /// <summary>
        /// The time this token was created
        /// </summary>
        public DateTime created_utc { get; set; }

        /// <summary>
        /// The OAUTH ID used to register this user
        /// </summary>
        public ObjectId oauth_application { get; set; }

        /// <summary>
        /// Scope flags set on this token. uint.MaxValue will ALWAYS grant permissions no matter the scope and should only be given to official applications
        /// IMPORTANT: This should only go up to 2^32, a uint32. However, MongoDB doesn't support this
        /// </summary>
        public ulong oauth_scope { get; set; }

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
}
