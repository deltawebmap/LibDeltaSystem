﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem
{
    public enum DeltaCoreNetServerType
    {
        MASTER_CONTROL = 1,

        API_MASTER = 11,
        API_ECHO = 12,
        API_SYNC = 13,
        API_RPC = 14,
        API_PRIMAL_PACKAGE_MANAGER = 15,
        API_SERVER_CONTENT_BUCKET = 19,
    }
}
