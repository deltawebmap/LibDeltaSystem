using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.RPC
{
    /// <summary>
    /// Used when sending requests to specify where messages should go
    /// </summary>
    public class RPCFilter
    {
        public string type; //"USER_ID", "SERVER", "TRIBE"
        public Dictionary<string, string> keys; //Params
    }
}
