using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbInventory : DbContentBase
    {
        /// <summary>
        /// The holder ID
        /// </summary>
        public ulong holder_id { get; set; }

        /// <summary>
        /// The type of the holder
        /// </summary>
        public DbInventory_InventoryType holder_type { get; set; }

        /// <summary>
        /// The current items
        /// </summary>
        public DbInventory_InventoryItem[] items { get; set; }

        public DateTime created_time { get; set; }

        public DateTime last_update_time { get; set; }

        public enum DbInventory_InventoryType
        {
            Dino = 0,
            Structure = 1,
        }

        public class DbInventory_InventoryItem
        {
            /// <summary>
            /// The ID of the item
            /// </summary>
            public ulong item_id { get; set; }

            /// <summary>
            /// Item classname
            /// </summary>
            public string classname { get; set; }

            /// <summary>
            /// Saved item durability
            /// </summary>
            public float durability { get; set; }

            /// <summary>
            /// Number of items in a stack
            /// </summary>
            public int stack_size { get; set; }

            /// <summary>
            /// Custom flags
            /// </summary>
            public ushort flags { get; set; }

            /// <summary>
            /// Custom data indexes
            /// </summary>
            public Dictionary<ushort, string> custom_data { get; set; }
        }
    }
}
