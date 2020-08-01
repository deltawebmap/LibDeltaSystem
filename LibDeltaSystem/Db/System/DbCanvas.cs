using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbCanvas : DbBaseSystem
    {
        /// <summary>
        /// Server ID associated with this
        /// </summary>
        public ObjectId server_id { get; set; }

        /// <summary>
        /// The tribe this belongs to
        /// </summary>
        public int tribe_id { get; set; }

        /// <summary>
        /// User IDs
        /// </summary>
        public List<ObjectId> users { get; set; }

        /// <summary>
        /// Data type version
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// User defined name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// User defined color
        /// </summary>
        public string color { get; set; }

        /// <summary>
        /// The last time this was saved.
        /// </summary>
        public DateTime last_saved { get; set; }

        /// <summary>
        /// The last time this was edited.
        /// </summary>
        public DateTime last_edited { get; set; }

        /// <summary>
        /// The last person to edit this
        /// </summary>
        public ObjectId last_editor { get; set; }

        /// <summary>
        /// Thumbnail on our user content server
        /// </summary>
        public string thumbnail_url { get; set; }

        /// <summary>
        /// Token to the thumbnail so that we can delete it
        /// </summary>
        public string thumbnail_token { get; set; }

        /// <summary>
        /// The time this thumbnail was saved
        /// </summary>
        public DateTime thumbnail_time { get; set; }

        /// <summary>
        /// Finds a canvas by it's ID and returns it
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<DbCanvas> GetCanvasById(DeltaConnection conn, ObjectId id)
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var result = await conn.system_canvases.FindAsync(filter);
            return await result.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Renames a canvas by changing it's name and color
        /// </summary>
        /// <returns></returns>
        public async Task UpdateUsers(DeltaConnection conn)
        {
            var updateBuilder = Builders<DbCanvas>.Update;
            var update = updateBuilder.Set("users", users);
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_canvases.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Sets a new thumbnail
        /// </summary>
        /// <param name="uc"></param>
        /// <returns></returns>
        public async Task SetNewThumbnail(DeltaConnection conn, Stream content)
        {
            //Create
            string thumbnail_url = await Tools.UserContentTool.UploadUserContentResizeImage(content, 256, 256);

            //Update
            var updateBuilder = Builders<DbCanvas>.Update;
            var update = updateBuilder.Set("thumbnail_url", thumbnail_url).Set("thumbnail_time", DateTime.UtcNow);
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            this.name = name;
            this.color = color;
            await conn.system_canvases.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Renames a canvas by changing it's name and color
        /// </summary>
        /// <returns></returns>
        public async Task RenameCanvas(DeltaConnection conn, string name, string color)
        {
            var updateBuilder = Builders<DbCanvas>.Update;
            var update = updateBuilder.Set("name", name).Set("color", color);
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            this.name = name;
            this.color = color;
            await conn.system_canvases.UpdateOneAsync(filter, update);
        }

        /// <summary>
        /// Deletes a canvas from the database
        /// </summary>
        /// <returns></returns>
        public async Task DeleteCanvas(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_canvases.DeleteOneAsync(filter);
        }
    }
}
