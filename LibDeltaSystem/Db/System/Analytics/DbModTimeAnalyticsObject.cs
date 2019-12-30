using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System.Analytics
{
    public class DbModTimeAnalyticsObject : DbBaseSystem
    {
        public DateTime time;
        public ObjectId server_id;
        public int client_version;
        public ObjectId client_session;
        public string key;
        public string action;
        public DbModTimeAnalyticsObject_Payload payload;

        public class DbModTimeAnalyticsObject_Payload
        {
            public float duration;
            public int length;
            public string extras;
        }
    }
}
