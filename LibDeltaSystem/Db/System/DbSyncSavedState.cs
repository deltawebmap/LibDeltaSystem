using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Represents a saved state for a connection to the sync server
    /// </summary>
    public class DbSyncSavedState
    {
        /// <summary>
        /// The code sent to the ARK server
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The time the session was created
        /// </summary>
        public DateTime time { get; set; }

        /// <summary>
        /// The version used by the mod
        /// </summary>
        public int mod_version { get; set; }

        /// <summary>
        /// The ID of the server this is connected to
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// The version of the sync server this was created on
        /// </summary>
        public int system_version { get; set; }
    }
}
