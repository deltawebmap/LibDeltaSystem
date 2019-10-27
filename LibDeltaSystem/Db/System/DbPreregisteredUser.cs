using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbPreregisteredUser : DbBaseSystem
    {
        public string email { get; set; }
        public DateTime time { get; set; }
    }
}
