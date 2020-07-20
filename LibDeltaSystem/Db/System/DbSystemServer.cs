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
        public int manager_id { get; set; }

        //Options used for manager servers

        public List<int> ports { get; set; } = new List<int>(); //Does NOT include the port used for corenet
        public Newtonsoft.Json.Linq.JObject config { get; set; } //Specialized config for custom use
    }
}
