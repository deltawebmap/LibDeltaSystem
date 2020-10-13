using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master.Entities
{
    public class NetManagerPackage
    {
        public string name;
        public string project_path;
        public string git_repo;
        public string exec;
        public int required_user_ports;
        public string latest_version;
        public string[] dependencies;
    }
}
