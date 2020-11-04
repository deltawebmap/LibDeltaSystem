using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master.Entities
{
    public class NetManagerInstance
    {
        public string package_name;
        public string version_id;
        public long id;
        public int[] ports;
        public string site_id; //May be null
    }
}
