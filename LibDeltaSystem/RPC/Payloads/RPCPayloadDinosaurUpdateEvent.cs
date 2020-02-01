using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
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
            public string dino_id;
            public DbDino dino;
            public DinosaurEntry species;
            public SavedDinoTribePrefs prefs;
        }
    }
}
