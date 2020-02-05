using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadAlertBannerPush : RPCPayload
    {
        public DbAlertBanner banner;
    }
}
