using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.ArkEntries
{
    public class DbArkEntry<T>
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }

        /// <summary>
        /// The mod ID
        /// </summary>
        public string mod { get; set; }

        /// <summary>
        /// The classname of this item
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// The time this was added
        /// </summary>
        public DateTime time { get; set; }

        /// <summary>
        /// The actual entry
        /// </summary>
        public T data { get; set; }
    }
}
