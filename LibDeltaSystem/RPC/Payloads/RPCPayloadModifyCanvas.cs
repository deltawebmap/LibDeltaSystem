using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadModifyCanvas : RPCPayload
    {
        public RPCPayloadModifyCanvas_ListedCanvas data;
        public RPCPayloadModifyCanvas_CanvasChange action;
        public RPCPayloadModifyCanvas_ActorUser user;

        public enum RPCPayloadModifyCanvas_CanvasChange
        {
            Create,
            Modify,
            Clear,
            Delete
        };

        public class RPCPayloadModifyCanvas_ListedCanvas
        {
            public string name;
            public string color;
            public string id;
            public string href;
            public string thumbnail;
        }

        public class RPCPayloadModifyCanvas_ActorUser
        {
            public string id;
            public string icon;
            public string name;
        }
    }
}
