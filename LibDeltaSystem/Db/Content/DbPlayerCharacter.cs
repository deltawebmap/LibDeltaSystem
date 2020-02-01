using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbPlayerCharacter : DbContentBase
    {
        public uint ark_id { get; set; }
        public DbVector3 pos { get; set; }
    }
}
