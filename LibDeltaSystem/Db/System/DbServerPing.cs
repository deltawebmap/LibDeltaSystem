using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbServerPing : DbBaseSystem
    {
        public ObjectId server_id { get; set; }
        public ObjectId session_id { get; set; }
        public float ticks_per_second { get; set; }
        public float avg_tick_seconds { get; set; }
        public float max_tick_seconds { get; set; }
        public float min_tick_seconds { get; set; }
        public int player_count { get; set; }
        public double game_delta { get; set; }
        public double ping_delta { get; set; }
        public DateTime time { get; set; }
    }
}
