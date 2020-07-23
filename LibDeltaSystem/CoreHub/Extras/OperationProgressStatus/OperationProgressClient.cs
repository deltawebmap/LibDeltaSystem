using LibDeltaSystem.CoreHub.CoreNetwork;
using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LibDeltaSystem.CoreHub.Extras.OperationProgressStatus
{
    public class OperationProgressClient
    {
        private ushort serverId;
        private ushort token;
        private BaseClientCoreNetwork network;
        private CoreNetworkServer server;
        private ushort nextLongMsgToken;

        public OperationProgressClient(BaseClientCoreNetwork network, uint token)
        {
            this.network = network;
            this.serverId = (ushort)((token & 0xFFFF0000) >> 16);
            this.token = (ushort)(token & 0x0000FFFF);
            server = network.list.GetServerById(serverId);
            if (server == null)
                throw new Exception("Could not find server ID requesting this!");
        }

        public void SendStatus(byte statusCode, string msg)
        {
            //Decide if this is a long message
            if(Encoding.UTF8.GetByteCount(msg) > OperationProgressServer.MAX_BUFFER_SIZE)
            {
                //Long message, sent in parts

                //Read bytes
                byte[] payload = Encoding.UTF8.GetBytes(msg);

                //Create token
                ushort longMsgToken = nextLongMsgToken;
                nextLongMsgToken++;

                //Create pilot message. It consists of the following format:
                //LEN   TYPE    NAME
                //2     UInt16  Token
                //1     Byte    Opcode (0x01)
                //1     Byte    User status code
                //2     UInt16  MsgToken
                //4     Int32   Total length of payload
                byte[] pilot = new byte[2 + 2 + 2 + 4];
                BinaryTool.WriteUInt16(pilot, 0, token);
                pilot[2] = 0x01;
                pilot[3] = statusCode;
                BinaryTool.WriteUInt16(pilot, 4, longMsgToken);
                BinaryTool.WriteInt32(pilot, 6, payload.Length);
                network.SendMessage(server, CoreNetworkOpcode.OPERATION_PROGRESS_UPDATED, pilot);

                //Wait to ensure the pilot is the first one the other end gets.
                //This is TERRIBLE code, but this will likely just be used for the admin panel. It's OK to drop packet chunks in that use case.
                Thread.Sleep(100);

                //Create template for the data packets. They all follow a similar format:
                //LEN   TYPE    NAME
                //2     UInt16  Token
                //1     Byte    Opcode (0x02)
                //1     Byte    *Buffer number
                //2     UInt16  MsgToken
                //2     UInt16  *Payload Chunk Length
                //CONST Binary  *Payload Data    
                //* Indicates that this will change with each chunk
                byte[] data = new byte[8 + OperationProgressServer.MAX_BUFFER_SIZE];
                BinaryTool.WriteUInt16(data, 0, token);
                data[2] = 0x02;
                data[3] = 0x00;
                BinaryTool.WriteUInt16(data, 4, longMsgToken);
                BinaryTool.WriteUInt16(data, 6, 0);

                //Create each data packet
                for(byte i = 0; i<(payload.Length / OperationProgressServer.MAX_BUFFER_SIZE) + 1; i++)
                {
                    //Copy
                    int payloadIndex = i * OperationProgressServer.MAX_BUFFER_SIZE;
                    int payloadWritten = Math.Min(payload.Length - payloadIndex, OperationProgressServer.MAX_BUFFER_SIZE);
                    Array.Copy(payload, payloadIndex, data, 8, payloadWritten);

                    //Write params
                    data[3] = i;
                    BinaryTool.WriteUInt16(data, 6, (ushort)payloadWritten);

                    //Send
                    byte[] dataCopy = new byte[data.Length];
                    data.CopyTo(dataCopy, 0);
                    network.SendMessage(server, CoreNetworkOpcode.OPERATION_PROGRESS_UPDATED, dataCopy);
                }
            }
            else
            {
                //Short message
                byte[] data = new byte[4 + Encoding.UTF8.GetByteCount(msg)];
                BinaryTool.WriteUInt16(data, 0, token);
                data[2] = 0x00; //opcode 0 - Short message
                data[3] = statusCode;
                Encoding.UTF8.GetBytes(msg, 0, msg.Length, data, 4);
                network.SendMessage(server, CoreNetworkOpcode.OPERATION_PROGRESS_UPDATED, data);
            }
        }
    }
}
