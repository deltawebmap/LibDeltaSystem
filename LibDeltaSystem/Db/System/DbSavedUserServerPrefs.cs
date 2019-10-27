using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Used for updating the last position of where the user is in the map, for example.
    /// </summary>
    public class DbSavedUserServerPrefs : DbBaseSystem
    {
        /// <summary>
        /// The ID of this server
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// User ID associated with this
        /// </summary>
        public string user_id { get; set; }

        /// <summary>
        /// Actual content
        /// </summary>
        public SavedUserServerPrefs payload { get; set; }
    }
}
