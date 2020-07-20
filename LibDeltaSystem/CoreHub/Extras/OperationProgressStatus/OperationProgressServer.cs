using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.Extras.OperationProgressStatus
{
    /// <summary>
    /// The end requesting the progress updates
    /// </summary>
    public class OperationProgressServer
    {
        private ushort token;

        public event OnProgressUpdatedEventArgs OnProgressUpdated;

        private static ushort next_token;

        public OperationProgressServer()
        {
            token = next_token;
            next_token++;
        }

        /// <summary>
        /// Called when we get a response operation
        /// </summary>
        /// <param name="token"></param>
        /// <param name="progressCode"></param>
        /// <param name="message"></param>
        public void OnOperationResponse(ushort token, short progressCode, string message)
        {
            //Validate
            if (this.token != token)
                return;

            //Call
            OnProgressUpdated(this, progressCode, message);
        }

        /// <summary>
        /// Attaches and returns a token to use when sending the event
        /// </summary>
        public uint Begin(BaseClientCoreNetwork network)
        {
            uint outputToken = ((uint)network.me.id << 16) | token;
            network.operationProgressServers.Add(this);
            return outputToken;
        }
        
        /// <summary>
        /// Detaches and ends the server
        /// </summary>
        /// <param name="network"></param>
        public void End(BaseClientCoreNetwork network)
        {
            network.operationProgressServers.Remove(this);
        }

        public delegate void OnProgressUpdatedEventArgs(OperationProgressServer source, short progressCode, string message);
    }
}
