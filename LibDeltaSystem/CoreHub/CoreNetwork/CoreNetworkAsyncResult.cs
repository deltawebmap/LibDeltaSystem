using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    public class CoreNetworkAsyncResult : IAsyncResult
    {
        public byte _status;
        public byte[] _result;
        public object _object;
        
        public CoreNetworkAsyncResult(byte status, byte[] result, object state)
        {
            _result = result;
            _status = status;
            _object = state;
        }

        public bool IsCompleted
        {
            get { return true; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public object AsyncState
        {
            get { return _object; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }
    }
}
