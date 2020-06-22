using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Represents a queued command for the injest server to send to ARK clients
    /// </summary>
    public class DbQueuedSyncRequest : DbBaseSystem
    {
        /// <summary>
        /// Does this persist beyond restarts?
        /// </summary>
        public bool persist { get; set; }

        /// <summary>
        /// The target server ID
        /// </summary>
        public ObjectId server_id { get; set; }

        /// <summary>
        /// The message opcode https://docs.google.com/spreadsheets/d/1Sa1uoDukka9kq7UIssVDJ3PoRCjDQK3y6IiX3iq6sU0/edit?usp=sharing
        /// </summary>
        public int opcode { get; set; }

        /// <summary>
        /// The custom payload data
        /// </summary>
        public string payload { get; set; }

        /// <summary>
        /// The user ID of the sender for this message
        /// </summary>
        public ObjectId sender_id { get; set; }

        public T DecodePayload<T>()
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }

        public JObject DecodePayloadAsJObject()
        {
            return DecodePayload<JObject>();
        }
    }
}
