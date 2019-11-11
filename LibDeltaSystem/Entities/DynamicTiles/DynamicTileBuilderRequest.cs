using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.DynamicTiles
{
    public class DynamicTileBuilderRequest
    {
        public DynamicTileTarget target;
        public bool highPriority;
        public int structure_revision_id;
    }
}
