using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetStructure
    {
        [WebFormatAttributeUseNameTable]
        public string classname;
        [WebFormatAttributeUseObject]
        public DbLocation location;
        public int structure_id;
        public bool has_inventory;
        public int tribe_id;
    }
}
