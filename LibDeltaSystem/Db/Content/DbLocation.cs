using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbLocation
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float pitch { get; set; }
        public float yaw { get; set; }
        public float roll { get; set; }

        public DbLocation()
        {

        }

        public DbLocation(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public DbLocation(float x, float y, float z, float pitch, float yaw, float roll)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }
    }
}
