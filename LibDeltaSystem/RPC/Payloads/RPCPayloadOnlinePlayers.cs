using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadOnlinePlayers : RPCPayload
    {
        public List<RPCPayloadOnlinePlayers_Player> players;
        public Dictionary<int, RPCPayloadOnlinePlayers_Tribe> tribes;
    }

    public class RPCPayloadOnlinePlayers_Player
    {
        public string name;
        public string icon;
        public int tribe_id;
    }

    public class RPCPayloadOnlinePlayers_Tribe
    {
        public string name;
    }
}
