using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using static LibDeltaSystem.Db.Content.DbInventory;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetInventory
    {
        public string holder_id { get; set; }
        public DbInventory_InventoryType holder_type { get; set; }
        public NetInventory_Item[] items { get; set; }
        public int tribe_id { get; set; }

        public class NetInventory_Item
        {
            public string item_id { get; set; }
            public string classname { get; set; }
            public float durability { get; set; }
            public int stack_size { get; set; }
            public ushort flags { get; set; }
            public Dictionary<ushort, string> custom_data { get; set; }
        }

        public static NetInventory ConvertInventory(DbInventory inv)
        {
            //Convert items
            NetInventory_Item[] items = new NetInventory_Item[inv.items.Length];
            for(var i = 0; i<inv.items.Length; i+=1)
            {
                items[i] = new NetInventory_Item
                {
                    classname = inv.items[i].classname,
                    durability = inv.items[i].durability,
                    item_id = inv.items[i].item_id.ToString(),
                    stack_size = inv.items[i].stack_size,
                    flags = inv.items[i].flags,
                    custom_data = inv.items[i].custom_data
                };
            }

            //Return full object
            return new NetInventory
            {
                holder_id = inv.holder_id.ToString(),
                holder_type = inv.holder_type,
                items = items,
                tribe_id = inv.tribe_id
            };
        }
    }
}
