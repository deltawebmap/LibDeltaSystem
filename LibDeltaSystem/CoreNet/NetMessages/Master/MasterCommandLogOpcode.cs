using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public enum MasterCommandLogOpcode
    {
        FINISHED_SUCCESS = 0,
        FINISHED_FAIL = 1,
        LOG = 10,
        LOG_CLI_BEGIN = 20,
        LOG_CLI_MESSAGE = 21,
        LOG_CLI_END = 22
    }
}
