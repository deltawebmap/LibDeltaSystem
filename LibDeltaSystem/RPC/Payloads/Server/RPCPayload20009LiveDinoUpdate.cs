using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20009LiveDinoUpdate : RPCPayload
    {
        public List<LiveDinoUpdateDino> dinos;

        public class LiveDinoUpdateDino
        {
            public string id;
            public Dictionary<int, float> stats;
        }
    }
}
