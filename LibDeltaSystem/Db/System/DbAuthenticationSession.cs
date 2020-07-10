using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbAuthenticationSession : DbBaseSystem
    {
        public string session_token { get; set; }
        public AuthState state { get; set; }
        public ObjectId application_id { get; set; }
        public string nonce { get; set; }
        public ulong scope { get; set; }
        public Dictionary<string, string> custom_data { get; set; }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbAuthenticationSession>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_auth_sessions.FindOneAndDeleteAsync(filter);
        }

        public async Task UpdateAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbAuthenticationSession>.Filter;
            var updateBuilder = Builders<DbAuthenticationSession>.Update;
            var filter = filterBuilder.Eq("_id", this._id);
            var update = updateBuilder.Set("state", state).Set("custom_data", custom_data);
            await conn.system_auth_sessions.UpdateOneAsync(filter, update);
        }

        public enum AuthState
        {
            PendingExternalAuth, //First status. It's while we're waiting for a user to sign in
            PendingOauthAuth, //We've signed in with Steam and are awaiting the oauth application to take over
        }
    }
}
