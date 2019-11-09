using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadDinosaurUpdateEvent : RPCPayload
    {
        public List<RPCPayloadDinosaurUpdateEvent_Dino> dinos;

        public class RPCPayloadDinosaurUpdateEvent_Dino
        {
            public string name;
            public string classname;
            public string icon;
            public int level;
            public string status;
            public float x;
            public float y;
            public float z;
            public string id;
        }
    }
}
