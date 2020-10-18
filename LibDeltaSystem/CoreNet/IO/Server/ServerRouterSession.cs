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

        public EndPoint RemoteEndPoint { get { return sock.RemoteEndPoint; } }

        public ServerRouterSession(IServerRouterIO server, Socket sock) : base(server, server.GetTransport(), server.GetAppVersion())
        {
            this.server = server;
            this.sock = sock;
            incomingBuffer = new byte[MESSAGE_TOTAL_SIZE];
        }

        public virtual string GetDebugName()
        {
            return $"[SOCK={sock.RemoteEndPoint.ToString()}]";
        }

        public void ListenIncoming()
        {
            sock.BeginReceive(incomingBuffer, 0, incomingBuffer.Length, SocketFlags.None, OnReceiveData, null);
        }

        private void OnReceiveData(IAsyncResult ar)
        {
            try
            {
                //Finish read
                int read = sock.EndReceive(ar);
                if (read == 0)
                    throw new Exception("Disconnected from client.");

                //Decode message
                int consumed = transport.DecodePacket(incomingBuffer, out RouterPacket packet);
                int notConsumed = read - consumed;

                //Handle
                _OnReceivePacket(packet);

                //Zero out the first four bytes of the buffer. This prevents us from rereading corrupted messages
                for (int i = 0; i < 4; i++)
                    incomingBuffer[i] = 0x00;

                //Listen for next
                for (var i = 0; i < notConsumed; i++)
                    incomingBuffer[i] = incomingBuffer[i + consumed];
                sock.BeginReceive(incomingBuffer, notConsumed, incomingBuffer.Length - notConsumed, SocketFlags.None, OnReceiveData, null);
            }
            catch (SocketException)
            {
                //Likely just disconnected. Drop quietly
                server.DropClient(this);
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
