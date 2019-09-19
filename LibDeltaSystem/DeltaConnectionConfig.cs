using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public class DeltaConnectionConfig
    {
        public string user;
        public string key;

        public string env;

        public int steam_cache_expire_minutes; //Number of minutes before a steam profile expires
        public string steam_api_token;

        public string server_ip;
        public int server_port;
    }
}
