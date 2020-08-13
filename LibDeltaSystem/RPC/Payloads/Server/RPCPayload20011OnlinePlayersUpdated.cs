using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20011OnlinePlayersUpdated : RPCPayload
    {
        public int player_count;
        public List<OnlinePlayer> players;

        public class OnlinePlayer
        {
            public int tribe_id;
            public string steam_name;
            public string steam_icon;
            public string steam_id;
        }
    }
}
