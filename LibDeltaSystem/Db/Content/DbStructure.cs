using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    public class DbStructure : DbContentBase
    {
        /// <summary>
        /// The location of this dinosaur
        /// </summary>
        public DbLocation location { get; set; }

        /// <summary>
        /// The classname of this object
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// Has an inventory or not
        /// </summary>
        public bool has_inventory { get; set; }

        /// <summary>
        /// If this structure has an inventory, the current number of items inside of it
        /// </summary>
        public int current_item_count { get; set; }

        /// <summary>
        /// If this structure has an inventory, the maximum number of items inside of it
        /// </summary>
        public int max_item_count { get; set; }

        /// <summary>
        /// The max health of the structure
        /// </summary>
        public float max_health { get; set; }

        /// <summary>
        /// The current health of this structure
        /// </summary>
        public float current_health { get; set; }

        /// <summary>
        /// Unique ID for this structure
        /// </summary>
        public int structure_id { get; set; }

        /// <summary>
        /// Used for version control
        /// </summary>
        public int revision_id { get; set; }

        /// <summary>
        /// Custom name set for this, if added
        /// </summary>
        public string custom_name { get; set; }

        /// <summary>
        /// Gets a structure by it's ID from a server
        /// </summary>
        /// <param name="token"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static async Task<DbStructure> GetStructureByID(DeltaConnection conn, int token, DbServer server)
        {
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = filterBuilder.Eq("structure_id", token);
            var response = await server.conn.content_structures.FindAsync(filter);
            var structure = await response.FirstOrDefaultAsync();
            return structure;
        }

        public bool TryGetItemEntry(DeltaConnection conn, DeltaPrimalDataPackage package, out ItemEntry entry)
        {
            entry = null;
            
            //Lookup structure metadata
            var metadata = conn.GetStructureMetadata().Where(x => x.names.Contains(classname)).FirstOrDefault();
            if (metadata != null)
            {
                //Get the item used for this
                entry = package.GetItemEntry(metadata.item);
                return entry != null;
            }
            return false;
        }
    }
}
