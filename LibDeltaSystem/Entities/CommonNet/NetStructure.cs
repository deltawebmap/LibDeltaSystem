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
        public string classname { get; set; }
        [WebFormatAttributeUseObject]
        public DbLocation location { get; set; }
        public int structure_id { get; set; }
        public bool has_inventory { get; set; }
        public int tribe_id { get; set; }
        public int commit_type { get; set; }
        public string commit_id { get; set; }
    }
}
