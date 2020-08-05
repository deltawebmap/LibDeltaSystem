using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public class DeltaConnectionConfig
    {
        public string env;

        public string mongodb_connection;  //MongoDB connection string

        public int steam_cache_expire_minutes; //Number of minutes before a steam profile expires
        public string steam_api_token;

        public string structure_metadata_config;

        public string firebase_config;
        public string firebase_uc_bucket;

        public DeltaConnectionConfig_Hosts hosts;
    }

    public class DeltaConnectionConfig_Hosts
    {
        public string master; //https://deltamap.net
        public string echo; //https://echo-content.deltamap.net
        public string assets_icon; //https://icon-assets.deltamap.net
        public string packages; //https://charlie-packages.deltamap.net
    }
}
