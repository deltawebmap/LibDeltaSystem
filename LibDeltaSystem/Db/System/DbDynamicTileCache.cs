using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbDynamicTileCache : DbBaseSystem
    {
        public DynamicTileTarget target { get; set; }

        public int tiles { get; set; }

        public string url { get; set; }

        public string server { get; set; }

        public DateTime create_time { get; set; }

        public int revision_id { get; set; }
    }
}
