﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbCanvas : DbBaseSystem
    {
        /// <summary>
        /// Server ID associated with this
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// User IDs
        /// </summary>
        public ObjectId[] users { get; set; }

        /// <summary>
        /// Index in the users array to begin writing
        /// </summary>
        public int user_index { get; set; }

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
        /// Sets a new thumbnail
        /// </summary>
        /// <param name="uc"></param>
        /// <returns></returns>
        public async Task SetNewThumbnail(DbUserContent uc)
        {
            //Attempt to remove the old thumbnail
            if(thumbnail_token != null)
            {
                DbUserContent old = await conn.GetUserContentByToken(thumbnail_token);
                if (old != null)
                    await old.DoDelete();
            }

            //Set details
            thumbnail_url = uc.url;
            thumbnail_token = uc.token;
            thumbnail_time = uc.upload_time;

            //Update
            var updateBuilder = Builders<DbCanvas>.Update;
            var update = updateBuilder.Set("thumbnail_url", thumbnail_url).Set("thumbnail_token", thumbnail_token).Set("thumbnail_time", thumbnail_time);
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
        public async Task RenameCanvas(string name, string color)
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
        public async Task DeleteCanvas()
        {
            var filterBuilder = Builders<DbCanvas>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_canvases.DeleteOneAsync(filter);
        }
    }
}