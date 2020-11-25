using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC.Payloads
{
    class RPCPayloadServerAccessChanged
    {
        public bool has_access;
        public bool is_admin;
        public bool has_tribe;
        public int? tribe_id;

        public RPCPayloadServerAccessChanged(bool is_admin, DbPlayerProfile profile)
        {
            has_access = is_admin || profile != null;
            this.is_admin = is_admin;
            has_tribe = profile != null;
            if (has_tribe)
                tribe_id = profile.tribe_id;
        }
    }
}
