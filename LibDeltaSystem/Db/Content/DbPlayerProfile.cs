using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Content
{
    public class DbPlayerProfile : DbContentBase
    {
        /// <summary>
        /// Player Steam name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// In game name
        /// </summary>
        public string ig_name { get; set; }

        /// <summary>
        /// Ark player ID
        /// </summary>
        public ulong ark_id { get; set; }

        /// <summary>
        /// The Steam ID used for this character
        /// </summary>
        public string steam_id { get; set; }

        /// <summary>
        /// Last time this person logged into the game
        /// </summary>
        public DateTime last_seen { get; set; }

        /// <summary>
        /// Steam icon
        /// </summary>
        public string icon { get; set; }
        
        /// <summary>
        /// The last X of the character - may be null
        /// </summary>
        public float? x { get; set; }

        /// <summary>
        /// The last Y of the character - may be null
        /// </summary>
        public float? y { get; set; }

        /// <summary>
        /// The last Z of the character - may be null
        /// </summary>
        public float? z { get; set; }

        /// <summary>
        /// The last yaw of the character - may be null
        /// </summary>
        public float? yaw { get; set; }

        public float? health { get; set; }
        public float? stamina { get; set; }
        public float? weight { get; set; }
        public float? food { get; set; }

        public static async Task DeleteServerContent(DeltaConnection conn, ObjectId server_id)
        {
            var filter = Builders<DbPlayerProfile>.Filter.Eq("server_id", server_id);
            await conn.content_player_profiles.DeleteOneAsync(filter);
        }
    }
}
