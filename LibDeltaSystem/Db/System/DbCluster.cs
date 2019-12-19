using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbCluster : DbBaseSystem
    {
        /// <summary>
        /// The owner of this cluster
        /// </summary>
        public ObjectId owner { get; set; }

        /// <summary>
        /// The name of this cluster
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets all clusters owned by a user
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<List<DbCluster>> GetClustersForUser(DeltaConnection conn, ObjectId user)
        {
            var filterBuilder = Builders<DbCluster>.Filter;
            var filter = filterBuilder.Eq("owner", user);
            var results = await conn.system_clusters.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Gets a cluster by it's id
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<DbCluster> GetClusterById(DeltaConnection conn, ObjectId id)
        {
            var filterBuilder = Builders<DbCluster>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var results = await conn.system_clusters.FindAsync(filter);
            return await results.FirstOrDefaultAsync();
        }
    }
}
