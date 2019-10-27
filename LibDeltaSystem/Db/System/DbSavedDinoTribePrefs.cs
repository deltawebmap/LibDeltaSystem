using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbSavedDinoTribePrefs : DbBaseSystem
    {
        /// <summary>
        /// The ID of this server
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// Dino ID associated with this
        /// </summary>
        public ulong dino_id { get; set; }

        /// <summary>
        /// The tribe ID associated with this
        /// </summary>
        public int tribe_id { get; set; }

        /// <summary>
        /// The actual content
        /// </summary>
        public SavedDinoTribePrefs payload { get; set; }
    }
}
