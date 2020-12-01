using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetGuildSettings
    {
        public string name;
        public int? permission_flags;
        public string permissions_template;
        public bool? is_secure;
        public bool? is_locked;
    }
}
