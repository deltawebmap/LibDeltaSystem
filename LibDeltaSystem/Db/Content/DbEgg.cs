using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    public class DbEgg : DbContentBase
    {
        /// <summary>
        /// ID of the underlying item
        /// </summary>
        public ulong item_id { get; set; }

        /// <summary>
        /// The maximum incubation temperature this egg supports
        /// </summary>
        public float max_temperature { get; set; }

        /// <summary>
        /// The minimum incubation temperature this egg supports
        /// </summary>
        public float min_temperature { get; set; }

        /// <summary>
        /// The current temperature of the egg
        /// </summary>
        public float current_temperature { get; set; }

        /// <summary>
        /// Egg health, on a scale of 1-0. 1 being max health
        /// </summary>
        public float health { get; set; }

        /// <summary>
        /// The incubation percent, on a scale of 1-0. 1 being first placed down, 0 being hatching
        /// </summary>
        public float incubation { get; set; }

        /// <summary>
        /// The time that this egg will hatch
        /// </summary>
        public DateTime hatch_time { get; set; }
        
        /// <summary>
        /// The time that this egg was last updated
        /// </summary>
        public DateTime updated_time { get; set; }

        /// <summary>
        /// The time that this egg was placed down at
        /// </summary>
        public DateTime placed_time { get; set; }

        /// <summary>
        /// The location of this egg on the ground
        /// </summary>
        public DbLocation location { get; set; }

        /// <summary>
        /// The classname of the resulting dino
        /// </summary>
        public string egg_type { get; set; }

        /// <summary>
        /// Parents string
        /// </summary>
        public string parents { get; set; }

        /// <summary>
        /// Sent notification types to avoid repeats
        /// </summary>
        public List<string> sent_notifications { get; set; }

        /// <summary>
        /// A salt, randomly generated, to allow cleanup of old responses
        /// </summary>
        public int updater_salt { get; set; }

        /// <summary>
        /// Gets the filter for a unique egg
        /// </summary>
        /// <returns></returns>
        public static FilterDefinition<DbEgg> GetFilterDefinition(string server_id, ulong id)
        {
            var filterBuilder = Builders<DbEgg>.Filter;
            var filter = filterBuilder.Eq("item_id", id) & filterBuilder.Eq("server_id", server_id);
            return filter;
        }

        /// <summary>
        /// Gets an egg by it'd ID
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<DbEgg> GetEggByItemID(DeltaConnection delta, string server_id, ulong id)
        {
            var filter = GetFilterDefinition(server_id, id);
            var result = await delta.content_eggs.FindAsync(filter);
            DbEgg c = await result.FirstOrDefaultAsync();
            if (c == null)
                return null;
            return c;
        }

        /// <summary>
        /// Gets an egg by it'd ID
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<DbEgg>> GetTribeEggs(DeltaConnection delta, string server_id, int tribe_id)
        {
            var filterBuilder = Builders<DbEgg>.Filter;
            var filter = filterBuilder.Eq("tribe_id", tribe_id) & filterBuilder.Eq("server_id", server_id);
            var result = await delta.content_eggs.FindAsync(filter);
            List<DbEgg> c = await result.ToListAsync();
            return c;
        }
    }
}
