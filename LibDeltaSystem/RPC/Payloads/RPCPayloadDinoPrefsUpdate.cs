using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    public class RPCPayloadDinoPrefsUpdate : RPCPayload
    {
        public string dino_id;
        public SavedDinoTribePrefs prefs;
        public string user_id;
    }
}
