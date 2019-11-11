using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.DynamicTiles
{
    public class DynamicTileBuilderResponse
    {
        public bool ok;
        public bool ready;
        public string url;
        public string server;
        public DynamicTileTarget target;
        public int count;
        public int revision_id;
    }
}
