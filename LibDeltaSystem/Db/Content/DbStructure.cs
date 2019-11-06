using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbStructure : DbContentBase
    {
        /// <summary>
        /// The location of this dinosaur
        /// </summary>
        public DbLocation location { get; set; }

        /// <summary>
        /// The classname of this object
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// Has an inventory or not
        /// </summary>
        public bool has_inventory { get; set; }

        /// <summary>
        /// If this structure has an inventory, the current number of items inside of it
        /// </summary>
        public int current_item_count { get; set; }

        /// <summary>
        /// If this structure has an inventory, the maximum number of items inside of it
        /// </summary>
        public int max_item_count { get; set; }

        /// <summary>
        /// The max health of the structure
        /// </summary>
        public float max_health { get; set; }

        /// <summary>
        /// The current health of this structure
        /// </summary>
        public float current_health { get; set; }

        /// <summary>
        /// Unique ID for this structure
        /// </summary>
        public int structure_id { get; set; }

        /// <summary>
        /// Used for version control
        /// </summary>
        public int revision_id { get; set; }
    }
}
