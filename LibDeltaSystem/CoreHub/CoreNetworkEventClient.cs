using LibDeltaSystem.CoreHub.CoreNetwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub
{
    public abstract class CoreNetworkEventClient : CoreNetworkFramework
    {
        private Dictionary<CoreNetworkOpcode, OnMessageHandlerEventArgs> events = new Dictionary<CoreNetworkOpcode, OnMessageHandlerEventArgs>();

        public void SubscribeMessageOpcode(CoreNetworkOpcode op, OnMessageHandlerEventArgs callback)
        {
            events.Add(op, callback);
        }

        public override byte[] OnMessage(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] payload)
        {
            if (events.ContainsKey(opcode))
                return events[opcode](server, opcode, payload);
            throw new Exception("Opcode is not registered; no handler to use!");
        }

        public delegate byte[] OnMessageHandlerEventArgs(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] payload);
    }
}
