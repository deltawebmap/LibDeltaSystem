using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Tools;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.ArkEntries
{
    public class DbPrimalPackage
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }

        /// <summary>
        /// The name of this object. A GUID
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The ID of the mod this is for. 0 is the base game
        /// </summary>
        public long mod_id { get; set; }

        /// <summary>
        /// The type of this package. Could be "SPECIES" or "ITEMS"
        /// </summary>
        public string package_type { get; set; }

        /// <summary>
        /// Time this was last updated
        /// </summary>
        public long last_updated { get; set; }

        public async Task UpdateModifiedTimeAsync(DeltaDatabaseConnection conn)
        {
            var filterBuilder = Builders<DbPrimalPackage>.Filter;
            var filter = filterBuilder.Eq("name", name);
            var update = Builders<DbPrimalPackage>.Update.Set("last_updated", DateTime.UtcNow.Ticks);
            await conn.arkentries_primal_packages.UpdateOneAsync(filter, update);
        }

        public FilterDefinition<DbArkEntry<T>> GetPrimalContentFilter<T>(int? lastEpoch)
        {
            var filterBuilder = Builders<DbArkEntry<T>>.Filter;
            var filter = filterBuilder.Eq("package_name", name);
            if (lastEpoch.HasValue)
                filter = filter & filterBuilder.Gt("time", TimeTool.GetTicksFromStandardEpoch(lastEpoch.Value));
            return filter;
        }

        public async Task<long> CountItemsAsync(DeltaConnection conn, int? lastEpoch)
        {
            //Switch on type
            if(package_type == "SPECIES")
            {
                //Get filter
                var filter = GetPrimalContentFilter<DinosaurEntry>(lastEpoch);

                //Get
                return await conn.arkentries_dinos.CountDocumentsAsync(filter);
            } else if (package_type == "ITEMS")
            {
                //Get filter
                var filter = GetPrimalContentFilter<ItemEntry>(lastEpoch);

                //Get
                return await conn.arkentries_items.CountDocumentsAsync(filter);
            } else
            {
                throw new Exception("Unknown Package Type");
            }
        }

        public Type GetPackageContentType()
        {
            if (package_type == "SPECIES")
                return typeof(DinosaurEntry);
            else if (package_type == "ITEMS")
                return typeof(ItemEntry);
            else
                throw new Exception("Unsupported Package Type");
        }

        public async Task<object[]> GetContentAsync(DeltaConnection conn, int? lastEpoch, int offset = 0, int limit = int.MaxValue)
        {
            //Switch on type
            if (package_type == "SPECIES")
            {
                //Get filter
                var filter = GetPrimalContentFilter<DinosaurEntry>(lastEpoch);

                //Get
                var r = await conn.arkentries_dinos.FindAsync(filter, new FindOptions<DbArkEntry<DinosaurEntry>, DbArkEntry<DinosaurEntry>>
                {
                    Limit = limit,
                    Skip = offset
                });

                //Convert
                var d = await r.ToListAsync();
                object[] response = new object[d.Count];
                for(int i = 0; i<response.Length; i+=1)
                {
                    response[i] = d[i].data;
                }
                return response;
            }
            else if (package_type == "ITEMS")
            {
                //Get filter
                var filter = GetPrimalContentFilter<ItemEntry>(lastEpoch);

                //Get
                var r = await conn.arkentries_items.FindAsync(filter, new FindOptions<DbArkEntry<ItemEntry>, DbArkEntry<ItemEntry>>
                {
                    Limit = limit,
                    Skip = offset
                });

                //Convert
                var d = await r.ToListAsync();
                object[] response = new object[d.Count];
                for (int i = 0; i < response.Length; i += 1)
                {
                    response[i] = d[i].data;
                }
                return response;
            }
            else
            {
                throw new Exception("Unknown Package Type");
            }
        }
    }
}
