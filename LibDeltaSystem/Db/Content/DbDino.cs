using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.CommonNet;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    public class DbDino : DbRevisionMappedContentBase
    {
        /// <summary>
        /// The ID of this dinosaur
        /// </summary>
        public ulong dino_id { get; set; }

        /// <summary>
        /// Is this dino tamed?
        /// </summary>
        public bool is_tamed { get; set; }

        /// <summary>
        /// Is this dino female?
        /// </summary>
        public bool is_female { get; set; }

        /// <summary>
        /// Color indexes of the dino. ALWAYS 6 items in size- regardless of actual size
        /// </summary>
        public int[] color_indexes { get; set; }

        /// <summary>
        /// The tamed name
        /// </summary>
        public string tamed_name { get; set; }

        /// <summary>
        /// The name of the person who originally tamed this dinosaur
        /// </summary>
        public string tamer_name { get; set; }

        /// <summary>
        /// The dinosaur classname
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// The current stats
        /// </summary>
        public float[] current_stats { get; set; }
        
        /// <summary>
        /// The max stats, sent from ARK
        /// </summary>
        public float[] max_stats { get; set; }

        /// <summary>
        /// The number of levelups applied
        /// </summary>
        public int[] base_levelups_applied { get; set; }

        /// <summary>
        /// The levelups applied while tamed
        /// </summary>
        public int[] tamed_levelups_applied { get; set; }

        /// <summary>
        /// The base level at spawn time
        /// </summary>
        public int base_level { get; set; }

        /// <summary>
        /// The level of this dinosaur
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// Exp
        /// </summary>
        public float experience { get; set; }

        /// <summary>
        /// Is this a baby dinosaur?
        /// </summary>
        public bool is_baby { get; set; }

        /// <summary>
        /// Not null if is_baby. 1 = fully grown, 0 = newborn
        /// </summary>
        public float baby_age { get; set; }

        /// <summary>
        /// The next imprint time if this is a baby dino
        /// </summary>
        public double next_imprint_time { get; set; }

        /// <summary>
        /// The baby dino imprint quality if this is a baby dino
        /// </summary>
        public float imprint_quality { get; set; }

        /// <summary>
        /// The location of this dinosaur
        /// </summary>
        public DbLocation location { get; set; }

        /// <summary>
        /// Agro status
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Taming effectiveness
        /// </summary>
        public float taming_effectiveness { get; set; }

        /// <summary>
        /// True if this dino is currently in a cryopod
        /// </summary>
        public bool is_cryo { get; set; }

        /// <summary>
        /// Current inventory this belongs to
        /// </summary>
        public string cryo_inventory_id { get; set; }

        /// <summary>
        /// Current inventory this belongs to
        /// </summary>
        public int cryo_inventory_type { get; set; }

        /// <summary>
        /// Current inventory this belongs to
        /// </summary>
        public ulong cryo_inventory_itemid { get; set; }

        /// <summary>
        /// ExperiencePoints
        /// </summary>
        public float experience_points { get; set; }

        /// <summary>
        /// The last time this was synced from ARK
        /// </summary>
        public DateTime last_sync_time { get; set; }

        /// <summary>
        /// The last time any attribute in here was updated
        /// </summary>
        public DateTime last_update_time { get; set; }

        /// <summary>
        /// Tribe prefs
        /// </summary>
        public SavedDinoTribePrefs prefs { get; set; }

        /// <summary>
        /// Is this dino alive?
        /// </summary>
        public bool is_alive { get; set; }

        public static ulong ZipperDinoId(uint id1, uint id2)
        {
            byte[] buf = new byte[8];
            BitConverter.GetBytes(id1).CopyTo(buf, 0);
            BitConverter.GetBytes(id2).CopyTo(buf, 4);
            return BitConverter.ToUInt64(buf, 0);
        }

        public static void UnzipperDinoId(ulong id, out uint id1, out uint id2)
        {
            byte[] buf = BitConverter.GetBytes(id);
            id1 = BitConverter.ToUInt32(buf, 0);
            id2 = BitConverter.ToUInt32(buf, 4);
        }

        /// <summary>
        /// Gets a dinosaur by it's ID from a server
        /// </summary>
        /// <param name="token"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static async Task<DbDino> GetDinosaurByID(DeltaDatabaseConnection conn, ulong token, DbServer server)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("dino_id", token) & filterBuilder.Eq("server_id", server._id);
            var response = await conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            return dino;
        }

        public async Task UpdatePrefs(DeltaConnection conn, DbServer server, SavedDinoTribePrefs prefs)
        {
            //TODO

            //Set here
            this.prefs = prefs;
        }

        public int GetBaseLevelUp(ArkDinoStat stat)
        {
            return base_levelups_applied[(int)stat];
        }

        public int GetTamedLevelUp(ArkDinoStat stat)
        {
            return tamed_levelups_applied[(int)stat];
        }

        public float GetCurrentStat(ArkDinoStat stat)
        {
            return current_stats[(int)stat];
        }

        public float GetMaxStat(ArkDinoStat stat)
        {
            return max_stats[(int)stat];
        }

        public static async Task DeleteServerContent(DeltaConnection conn, ObjectId server_id)
        {
            var filter = Builders<DbDino>.Filter.Eq("server_id", server_id);
            await conn.content_dinos.DeleteOneAsync(filter);
        }
    }
}
