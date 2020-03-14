using LibDeltaSystem.RPC.Payloads.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20002PartialUpdate : RPCPayload
    {
        public string id;
        public RPCSyncType type;
        public DateTime time;
        public int tribe_id;
        public RPCPayload20002PartialUpdate_Update updates;

        public class RPCPayload20002PartialUpdate_Update
        {
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
