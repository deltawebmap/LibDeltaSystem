using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetTribe
    {
        public string tribe_name;
        public DateTime last_seen;
        public int tribe_id;

        public static NetTribe ConvertTribe(DbTribe tribe)
        {
            return new NetTribe
            {
                last_seen = tribe.last_seen,
                tribe_id = tribe.tribe_id,
                tribe_name = tribe.tribe_name
            };
        }
    }
}
