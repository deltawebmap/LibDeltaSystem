using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.PrivateNet
{
    public class SteamProfile
    {
        public string steamid { get; set; }
        public string profilestate { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatarfull { get; set; }
        public long timecreated { get; set; }
    }

    public class SteamProfile_Players
    {
        public List<SteamProfile> players;
    }

    public class SteamProfile_Full
    {
        public SteamProfile_Players response;
    }
}
