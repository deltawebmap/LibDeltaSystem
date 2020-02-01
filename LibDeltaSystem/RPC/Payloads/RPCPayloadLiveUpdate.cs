using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadLiveUpdate : RPCPayload
    {
        public List<RPCLiveUpdateData> updates;

        public class RPCLiveUpdateData
        {
            public int type;
            public string id;

            public float? x;
            public float? y;
            public float? z;
            public float? yaw;
            public float? health;
            public float? stamina;
            public float? weight;
            public float? food;
        }
    }
}
