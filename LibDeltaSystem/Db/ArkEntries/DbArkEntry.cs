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
        /// Classname of this item
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// The name of the package this belongs to
        /// </summary>
        public string package_name { get; set; }

        /// <summary>
        /// The time this was uploaded
        /// </summary>
        public long time { get; set; }

        /// <summary>
        /// The actual entry
        /// </summary>
        public T data { get; set; }
    }
}
