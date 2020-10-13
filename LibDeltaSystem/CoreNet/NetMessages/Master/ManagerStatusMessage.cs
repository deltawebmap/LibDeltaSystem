using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerStatusMessage
    {
        public string host_name;
        public string host_os;
        public byte version_lib_major;
        public byte version_lib_minor;
        public byte version_app_major;
        public byte version_app_minor;
        public DateTime start_time;
        public DateTime current_time;
    }
}
