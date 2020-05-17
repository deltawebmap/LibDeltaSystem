using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbBetaKey : DbBaseSystem
    {
        public string key { get; set; }
        public string note { get; set; }
        public ObjectId claimed_by { get; set; }
        public bool claimed { get; set; }
    }
}
