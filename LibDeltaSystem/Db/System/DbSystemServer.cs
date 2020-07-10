using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// A Delta Web Map server
    /// </summary>
    public class DbSystemServer : DbBaseSystem
    {
        public int server_id { get; set; } //Actually a ushort
        public string server_token { get; set; } //Actually a ulong
        public string server_type { get; set; } //The type enum
        public string address { get; set; }
        public int port { get; set; }
        public string enviornment { get; set; }
    }
}
