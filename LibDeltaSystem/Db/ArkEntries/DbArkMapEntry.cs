using LibDeltaSystem.Entities.ArkEntries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.ArkEntries
{
    public class DbArkMapEntry
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }

        /// <summary>
        /// The internal name used by ARK to store this data
        /// </summary>
        public string internalName { get; set; }

        /// <summary>
        /// The actual content stored
        /// </summary>
        public ArkMapEntry data { get; set; }
    }
}
