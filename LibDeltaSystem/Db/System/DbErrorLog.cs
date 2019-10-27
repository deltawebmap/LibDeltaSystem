using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbErrorLog : DbBaseSystem
    {
        public string message { get; set; }
        public bool is_standard { get; set; }
        public string stack_trace { get; set; }
        public DateTime time { get; set; }
        public string system { get; set; }
        public int system_version_minor { get; set; }
        public int system_version_major { get; set; }
        public Dictionary<string, string> extras { get; set; }
    }
}
