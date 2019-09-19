using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbSteamCache : DbBaseSystem
    {
        public string name { get; set; }

        public string steam_id { get; set; }

        public string icon_url { get; set; }

        public string profile_url { get; set; }

        public long time_utc { get; set; }
    }
}
