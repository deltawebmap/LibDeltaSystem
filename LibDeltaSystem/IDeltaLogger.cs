using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public interface IDeltaLogger
    {
        void Log(string topic, string message, DeltaLogLevel level);
    }
}
