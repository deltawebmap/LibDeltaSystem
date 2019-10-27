using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.PrivateNet
{
    public class SteamModDataResponse
    {
        public int result { get; set; }
        public int resultcount { get; set; }
        public List<DbSteamModCache> publishedfiledetails { get; set; }
    }

    public class SteamModDataRootObject
    {
        public SteamModDataResponse response { get; set; }
    }
}
