using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbTribeLogEntry : DbContentBase
    {
        /// <summary>
        /// Actual data
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// Color code
        /// </summary>
        public string color { get; set; }

        /// <summary>
        /// Has a color set 
        /// </summary>
        public bool has_color { get; set; }

        /// <summary>
        /// In-Game day
        /// </summary>
        public int day { get; set; }

        /// <summary>
        /// In-Game hour
        /// </summary>
        public int hour { get; set; }

        /// <summary>
        /// In-Game min
        /// </summary>
        public int min { get; set; }

        /// <summary>
        /// In-Game sec
        /// </summary>
        public int sec { get; set; }

        /// <summary>
        /// When this was first seen
        /// </summary>
        public DateTime seen { get; set; }

        /// <summary>
        /// False when the system detects that this is an old event that was only found now
        /// </summary>
        public bool realtime { get; set; }

        /// <summary>
        /// Index, based on time and other factors. Just used for sorting
        /// </summary>
        public long index { get; set; }
    }
}
