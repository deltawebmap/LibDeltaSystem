using LibDeltaSystem.Entities.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadPutNotification : RPCPayload
    {
        public PushNotification notification;
    }
}
