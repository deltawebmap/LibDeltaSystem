using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.Entities
{
    public class DeltaWebServerStatus
    {
        public DateTime start;
        public long uptime;
        public string enviornment;
        public bool debug_mode;
        public string server_version;
        public string lib_version;
        public DeltaConnectionConfig_Hosts hosts;
        public string server_type;
        public int server_id;
    }
}
