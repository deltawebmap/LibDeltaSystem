using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Base class that allows delete, update, and other functions
    /// </summary>
    public abstract class DbBaseSystem
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }

        /// <summary>
        /// The ID that is easy to access
        /// </summary>
        [BsonIgnore]
        public string id { get { return _id.ToString(); } }
    }
}
