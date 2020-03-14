using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetMiniUser
    {
        public string icon;
        public string id;
        public string name;

        public static NetMiniUser ConvertUser(DbUser u)
        {
            return new NetMiniUser
            {
                icon = u.profile_image_url,
                id = u.id,
                name = u.screen_name
            };
        }
    }
}
