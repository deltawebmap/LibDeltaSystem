using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    class CoreNetworkOutgoingMessage
    {
        public CoreNetworkServer server;
        public uint id;
        public CoreNetworkOpcode opcode;
        public byte[] payload;
        public DateTime lastSent;
        public AsyncCallback callback;
        public object asyncState;
        public bool ackSendRequired = true; //Set to true initially, else manually
        public ulong ackMessageId = 0; //IF this is an ack, this will hold the global ID of the message to be ack'd. This is used for resending ACKs
        public ulong globalId { get { return ((ulong)server.id << 32) | id; } } //An ID unique to all servers

        public bool CanSend()
        {
            //ACK messages aren't automatically resent. They're also removed after some amount of time not being requested
            if (opcode == CoreNetworkOpcode.MESSAGE_ACK)
            {
                //Only send once unless we're manually overwriting it
                if(ackSendRequired)
                {
                    ackSendRequired = false;
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                //Resend after an amount of time
                return (DateTime.UtcNow - lastSent).TotalSeconds > 10;
            }
        }
    }
}
