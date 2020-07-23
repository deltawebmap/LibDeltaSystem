using LibDeltaSystem.Tools;
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
        public ushort token;
        public event OnProgressUpdatedEventArgs OnProgressUpdated;

        private Dictionary<ushort, LongMsgData> longMessages; //In-Progress long messages

        private static ushort next_token;

        public const ushort MAX_BUFFER_SIZE = 16384; //messages larger than this will be sent in multiple parts

        public OperationProgressServer()
        {
            token = next_token;
            next_token++;
            longMessages = new Dictionary<ushort, LongMsgData>();
        }

        /// <summary>
        /// Called when we get a response operation
        /// </summary>
        /// <param name="token"></param>
        /// <param name="progressCode"></param>
        /// <param name="message"></param>
        public void OnOperationResponse(byte[] payload)
        {
            //Check opcode
            switch(payload[2])
            {
                case 0x00: OnShortMsgRespond(payload); break;
                case 0x01: OnLongMsgPilot(payload); break;
                case 0x02: OnLongMsgChunk(payload); break;
            }
        }

        private void OnShortMsgRespond(byte[] payload)
        {
            //Read
            string data = Encoding.UTF8.GetString(payload, 4, payload.Length - 4);

            //Call
            OnProgressUpdated(this, payload[3], data);
        }

        private void OnLongMsgPilot(byte[] payload)
        {
            //Read
            byte userCode = payload[3];
            ushort msgToken = BinaryTool.ReadUInt16(payload, 4);
            int payloadLength = BinaryTool.ReadInt32(payload, 6);

            //Calculate
            byte chunks = (byte)((payloadLength / MAX_BUFFER_SIZE) + 1);

            //Create
            byte[] buffer = new byte[payloadLength];
            longMessages.Add(msgToken, new LongMsgData
            {
                chunksRemaining = chunks,
                payload = buffer,
                totalPayloadLength = payloadLength,
                userCode = userCode
            });
        }

        private void OnLongMsgChunk(byte[] payload)
        {
            //Read
            byte bufferNumber = payload[3];
            ushort msgToken = BinaryTool.ReadUInt16(payload, 4);
            ushort chunkLength = BinaryTool.ReadUInt16(payload, 6);

            //Copy
            Array.Copy(payload, 8, longMessages[msgToken].payload, MAX_BUFFER_SIZE * bufferNumber, chunkLength);
            longMessages[msgToken].chunksRemaining--;

            //Check if completed
            if(longMessages[msgToken].chunksRemaining == 0)
            {
                //Call
                OnProgressUpdated(this, longMessages[msgToken].userCode, Encoding.UTF8.GetString(longMessages[msgToken].payload));

                //Clean up
                longMessages.Remove(msgToken);
            }
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

        class LongMsgData
        {
            public byte userCode;
            public byte[] payload;
            public byte chunksRemaining;
            public int totalPayloadLength;
        }
    }
}
