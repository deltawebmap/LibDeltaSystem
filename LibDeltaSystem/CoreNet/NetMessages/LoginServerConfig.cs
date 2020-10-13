using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages
{
    public class LoginServerConfig
    {
        public string enviornment;
        public string mongodb_connection;
        public string steam_api_key;
        public int steam_cache_expire_minutes;
        public string firebase_uc_bucket;
        public bool log;
        public string steam_token_key; //The key we use to encrypt Steam tokens with. 16 bytes of Base64 data
        public LoginServerConfigHosts hosts;
    }

    public class LoginServerConfigHosts
    {
        public string master;
        public string echo;
        public string assets_icon;
        public string packages;
    }
}
