using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using static LibDeltaSystem.Db.Content.DbInventory;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetInventory
    {
        public string holder_id { get; set; }
        [WebFormatAttributeUseNameTable]
        public string holder_type { get; set; }
        [WebFormatAttributeUseObject]
        public NetInventory_Item[] items { get; set; }
        public int tribe_id { get; set; }
        public int commit_type { get; set; }
        [WebFormatAttributeUseNameTable]
        public string commit_id { get; set; }

        public class NetInventory_Item
        {
            public string item_id { get; set; }
            [WebFormatAttributeUseNameTable]
            public string classname { get; set; }
            public float durability { get; set; }
            public int stack_size { get; set; }
            public ushort flags { get; set; }

            public static NetInventory_Item ConvertItem(DbInventory_InventoryItem item)
            {
                return new NetInventory_Item
                {
                    classname = item.classname,
                    durability = item.durability,
                    item_id = item.item_id.ToString(),
                    stack_size = item.stack_size,
                    flags = item.flags
                };
            }
        }

        public static NetInventory ConvertInventory(DbInventory inv)
        {
            //Convert items
            NetInventory_Item[] items = new NetInventory_Item[inv.items.Length];
            for(var i = 0; i<inv.items.Length; i+=1)
            {
                items[i] = NetInventory_Item.ConvertItem(inv.items[i]);
            }

            //Return full object
            return new NetInventory
            {
                holder_id = inv.holder_id.ToString(),
                holder_type = inv.holder_type.ToString().ToUpper(),
                items = items,
                tribe_id = inv.tribe_id
            };
        }
    }
}
