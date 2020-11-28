using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO.Server
{
    public class ServerRouterSession : BaseRouterIO
    {
        internal Socket sock;
        private IServerRouterIO server;
        private byte[] incomingBuffer;
        private int incomingOffset;

        public EndPoint RemoteEndPoint { get { return sock.RemoteEndPoint; } }

        public ServerRouterSession(IServerRouterIO server, Socket sock) : base(server, server.GetTransport(), server.GetAppVersion())
        {
            this.server = server;
            this.sock = sock;
            incomingBuffer = new byte[transport.GetFrameSize()];
        }

        public virtual string GetDebugName()
        {
            return $"[SOCK={sock.RemoteEndPoint.ToString()}]";
        }

        public void ListenIncoming()
        {
            sock.BeginReceive(incomingBuffer, incomingOffset, incomingBuffer.Length - incomingOffset, SocketFlags.None, OnReceiveData, null);
        }

        private void OnReceiveData(IAsyncResult ar)
        {
            try
            {
                //Finish read
                int read = sock.EndReceive(ar);
                incomingOffset += read;
                if (read == 0)
                    throw new Exception("Disconnected from client.");

                //Check if we have a full packet
                if (read == incomingBuffer.Length)
                {
                    //Decode
                    transport.DecodePacket(incomingBuffer, out RouterPacket packet);

                    //Reset state
                    incomingOffset = 0;

                    //Handle
                    _OnReceivePacket(packet);

                    //Zero out the first four bytes of the buffer. This prevents us from rereading corrupted messages
                    for (int i = 0; i < 4; i++)
                        incomingBuffer[i] = 0x00;
                }

                //Listen
                ListenIncoming();
            }
            catch (Exception ex)
            {
                //Unknown error. Drop with log
                Log("OnReceiveData", $"Hit exception handling incoming data: {ex.Message}{ex.StackTrace}. Dropping client...", DeltaLogLevel.Medium);
                server.DropClient(this);
            }
        }

        public override void QueueOutgoingMessage(RouterPacket packet)
        {
            server.SendClientPacket(this, packet);
        }

        public override void RouterReceiveMessage(RouterMessage msg)
        {
            server.OnIncomingClientMessage(this, msg);
        }
    }
}
