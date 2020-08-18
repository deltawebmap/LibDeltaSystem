using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public class DeltaConnectionConfig
    {
        public int version; //Version of the config
        public string env;
        public string mongodb_connection;  //MongoDB connection string
        public int steam_cache_expire_minutes; //Number of minutes before a steam profile expires
        public string steam_api_token;
        public string configs_location; //Folder containing configs
        public string firebase_uc_bucket;
        public bool log; //If set to false, no remote logging will happen for error levels less than high. No error levels will be written to stdout

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
