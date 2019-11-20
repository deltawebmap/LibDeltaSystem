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

        public string rpc_key; //64 bytes of Base-64 encoded content to be kept as a private key
        public int rpc_port; //Port to use when communicating with the RPC
        public string rpc_ip; //RPC port. MUST be an actual port, not a hostname

        public int steam_cache_expire_minutes; //Number of minutes before a steam profile expires
        public string steam_api_token;

        public string server_ip;
        public int server_port;

        public string firebase_config; //Path to firebase config file
        public string structure_metadata_config;
    }
}
