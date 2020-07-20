using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// A Delta Web Map server
    /// </summary>
    public class DbSystemAdminUser : DbBaseSystem
    {
        public string username { get; set; }
        public byte[] salt { get; set; }
        public byte[] password_hash { get; set; }
    }
}
