using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.WebFramework.WebSockets.Groups
{
    public abstract class WebSocketGroupQuery
    {
        /// <summary>
        /// Checks if a requested query matches this query
        /// </summary>
        /// <param name="request"></param>
        public abstract bool CheckIfAuthorized(WebSocketGroupQuery request);
    }
}
