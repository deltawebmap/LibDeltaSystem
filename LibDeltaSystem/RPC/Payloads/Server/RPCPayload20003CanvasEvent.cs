using LibDeltaSystem.Entities.CommonNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads.Server
{
    public class RPCPayload20003CanvasEvent : RPCPayload
    {
        public NetCanvas data;
        public NetMiniUser user;
        public RPCPayload20003CanvasEvent_CanvasEventType action;

        public enum RPCPayload20003CanvasEvent_CanvasEventType
        {
            Create = 0,
            Modify = 1,
            Delete = 2,

        }
    }
}
