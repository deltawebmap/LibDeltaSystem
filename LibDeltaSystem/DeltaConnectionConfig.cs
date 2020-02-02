using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public class DeltaConnectionConfig
    {
        public string env;
        public string mongodb_connection; //MongoDB connection string

        public string rpc_key; //64 bytes of Base-64 encoded content to be kept as a private key
        public int rpc_port; //Port to use when communicating with the RPC
        public string rpc_ip; //RPC port. MUST be an actual port, not a hostname

        public int steam_cache_expire_minutes; //Number of minutes before a steam profile expires
        public string steam_api_token;

        public string structure_metadata_config;

        public bool debug_mode;

        public DeltaConnectionConfig_Hosts hosts;
    }

    public class DeltaConnectionConfig_Hosts
    {
        public string master; //https://deltamap.net
        public string echo; //https://echo-content.deltamap.net
        public string assets_icon; //https://icon-assets.deltamap.net
    }
}
