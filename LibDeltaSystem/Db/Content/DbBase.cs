using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LibDeltaSystem.Db.Content
{
    /// <summary>
    /// Contains some basic data that will always be written to the database
    /// </summary>
    public class DbContentBase
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        [JsonIgnore]
        public ObjectId _id { get; set; }

        /// <summary>
        /// Server this dinosaur belongs to
        /// </summary>
        [JsonIgnore]
        public ObjectId server_id { get; set; }

        /// <summary>
        /// The tribe ID this dinosaur belongs to
        /// </summary>
        public int tribe_id { get; set; }
    }
}
