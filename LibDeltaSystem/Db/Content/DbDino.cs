using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    public class DbDino : DbContentBase
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
        /// Colors in hex format
        /// </summary>
        public string[] colors { get; set; }

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
        public DbArkDinosaurStats current_stats { get; set; }
        
        /// <summary>
        /// The max stats, sent from ARK
        /// </summary>
        public DbArkDinosaurStats max_stats { get; set; }

        /// <summary>
        /// The number of levelups applied
        /// </summary>
        public DbArkDinosaurStats base_levelups_applied { get; set; }

        /// <summary>
        /// The levelups applied while tamed
        /// </summary>
        public DbArkDinosaurStats tamed_levelups_applied { get; set; }

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
        /// Revision ID this is mapped to
        /// </summary>
        public int revision_id { get; set; }

        /// <summary>
        /// Returns this dino's prefs. Will never return null.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public async Task<SavedDinoTribePrefs> GetPrefs(DeltaConnection conn)
        {
            return await conn.GetDinoPrefs(server_id, tribe_id, dino_id);
        }

        /// <summary>
        /// Gets a dinosaur by it's ID from a server
        /// </summary>
        /// <param name="token"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public static async Task<DbDino> GetDinosaurByID(DeltaConnection conn, ulong token, DbServer server)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("dino_id", token) & filterBuilder.Eq("server_id", server.id);
            var response = await server.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            return dino;
        }
    }
}
