using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork.Exceptions
{
    public class CoreNetworkRemoteErrorException : Exception
    {
        public string remoteError;
        
        public CoreNetworkRemoteErrorException(string data) : base("The remote server threw an exception while attempting to process your request.")
        {
            this.remoteError = data;
        }
    }
}
