﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList
{
    public abstract class ICoreNetworkServerList
    {
        public abstract CoreNetworkServer GetServerById(ushort id);
    }
}