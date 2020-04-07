using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetStructure
    {
        public string classname;
        public DbLocation location;
        public int structure_id;
        public bool has_inventory;
        public int tribe_id;
    }
}
