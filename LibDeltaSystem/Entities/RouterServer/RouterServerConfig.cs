using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.RouterServer
{
    public class RouterServerConfig
    {
        public string label;
        public short id;
        public byte[] auth_key;
        public string master_ip;
        public int master_port;
    }
}
