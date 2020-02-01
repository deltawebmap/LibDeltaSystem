using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public class TimeTool
    {
        public DateTime GetDateTimeFromEpoch(long time)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time);
        }

        public long GetEpochFromDateTime(DateTime time)
        {
            return (long)(time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
