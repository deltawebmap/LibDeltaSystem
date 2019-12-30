using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Represents a saved state for a connection to the sync server
    /// </summary>
    public class DbSyncSavedState : DbBaseSystem
    {
        /// <summary>
        /// The code sent to the ARK server
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The time the session was created
        /// </summary>
        public DateTime time { get; set; }

        /// <summary>
        /// The version used by the mod
        /// </summary>
        public int mod_version { get; set; }

        /// <summary>
        /// The mod enviornment. Usually PRODUCTION
        /// </summary>
        public string mod_enviornment { get; set; }

        /// <summary>
        /// The ID of the server this is connected to
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// The version of the sync server this was created on
        /// </summary>
        public int system_version { get; set; }

        /// <summary>
        /// Gets a DbSyncSavedState object.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<DbSyncSavedState> GetStateByTokenAsync(DeltaConnection conn, string token)
        {
            var filterBuilder = Builders<DbSyncSavedState>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var results = await conn.system_sync_states.FindAsync(filter);
            var r = await results.FirstOrDefaultAsync();
            return r;
        }
    }
}
