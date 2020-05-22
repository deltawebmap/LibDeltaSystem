using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    /// <summary>
    /// Tribe that sits in it's own file on the disk
    /// </summary>
    public class DbTribe : DbContentBase
    {
        /// <summary>
        /// Name of the tribe
        /// </summary>
        public string tribe_name { get; set; }

        /// <summary>
        /// Owner of the tribe
        /// </summary>
        public uint tribe_owner { get; set; }

        /// <summary>
        /// Last time this tribe was seen online
        /// </summary>
        public DateTime last_seen { get; set; }

        public static async Task DeleteServerContent(DeltaConnection conn, ObjectId server_id)
        {
            var filter = Builders<DbTribe>.Filter.Eq("server_id", server_id);
            await conn.content_tribes.DeleteOneAsync(filter);
        }
    }
}
