using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities
{
    /// <summary>
    /// A token that can be used to look up Steam IDs
    /// </summary>
    public class SteamIdToken
    {
        public string steam_id;
        public byte version;
    }
}
