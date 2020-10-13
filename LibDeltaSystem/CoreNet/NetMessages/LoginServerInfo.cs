using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages
{
    public class LoginServerInfo
    {
        public bool success;
        public string instance_id;
        public LoginServerConfig config;
        public int[] user_ports;
    }
}
