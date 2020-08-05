using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public class TimeTool
    {
        //Jan 1, 2020 at midnight UTC
        public const long MASTER_EPOCH = 637134336000000000;
        public const int CONVERSION_FACTOR = 10000000; //Number of ticks in one second

        public static int GetStandardEpochFromTicks(long ticks)
        {
            return (int)((ticks - MASTER_EPOCH) / CONVERSION_FACTOR);
        }

        public static long GetTicksFromStandardEpoch(int epoch)
        {
            return ((long)epoch * CONVERSION_FACTOR) + MASTER_EPOCH;
        }
    }
}
