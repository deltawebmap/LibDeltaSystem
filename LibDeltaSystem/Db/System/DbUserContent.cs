using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbUserContent : DbBaseSystem
    {
        /// <summary>
        /// Image URL
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Name in URL
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Token that can be used to verify this or delete the image
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// User ID that uploaded this asset
        /// </summary>
        public ObjectId uploader { get; set; }

        /// <summary>
        /// Size, in bytes, of the image
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// MIME type of the image
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Time this was uploaded
        /// </summary>
        public DateTime upload_time { get; set; }

        /// <summary>
        /// The server this was uploaded to
        /// </summary>
        public string node_name { get; set; }

        /// <summary>
        /// The application this was uploaded to
        /// </summary>
        public string application_id { get; set; }

        /// <summary>
        /// Flags this image for removal
        /// </summary>
        public bool deleted { get; set; }

        /// <summary>
        /// Flags this image for removal
        /// </summary>
        /// <returns></returns>
        public async Task DoDelete()
        {
            var updateBuilder = Builders<DbUserContent>.Update;
            var update = updateBuilder.Set("deleted", true);
            var filterBuilder = Builders<DbUserContent>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            this.deleted = true;
            await conn.system_user_uploads.UpdateOneAsync(filter, update);
        }
    }
}
