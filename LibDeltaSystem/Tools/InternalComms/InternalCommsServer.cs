using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools.InternalComms
{
    /// <summary>
    /// Server for server to server communications in our backend.
    /// </summary>
    public abstract class InternalCommsServer
    {
        public DeltaConnection delta;
        public Socket server;
        public byte[] key;
        public int port;

        public InternalCommsServer(DeltaConnection conn, byte[] key, int port)
        {
            this.delta = conn;
            this.key = key;
            this.port = port;
        }

        public void StartServer()
        {
            //Start server
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(32);
            server.BeginAccept(OnAcceptConnection, null);
        }

        private void OnAcceptConnection(IAsyncResult r)
        {
            //Get state
            Socket sock = server.EndAccept(r);

            //Create new connection
            InternalCommsServerClient conn = GetClient(delta, key, sock);

            //Create new salt to use
            byte[] salt = SecureStringTool.GenerateSecureRandomBytes(32);

            //Send auth request
            conn.RawSendMessage(-1, new Dictionary<string, byte[]>
            {
                {"SALT", salt }
            });

            //Set salt
            conn.SetSalt(salt);

            //Get new messages
            conn.BeginReceiveMessage();

            //Accept next
            server.BeginAccept(OnAcceptConnection, null);
        }

        /// <summary>
        /// Called when a client is connected. Allows the user to, optionally, return a child type
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="key"></param>
        /// <param name="sock"></param>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        public abstract InternalCommsServerClient GetClient(DeltaConnection conn, byte[] key, Socket sock);

        /// <summary>
        /// Called when a client has been authorized and is ready to go
        /// </summary>
        public abstract void OnClientAuthorized(InternalCommsServerClient client);

        /// <summary>
        /// Called when a client has been disconnected
        /// </summary>
        public abstract void OnClientDisconnected(InternalCommsServerClient client);

        /* Client */

        public abstract class InternalCommsServerClient : InternalCommBase
        {
            public InternalCommsServer server;

            public InternalCommsServerClient(DeltaConnection conn, byte[] key, Socket sock, InternalCommsServer server) : base(conn, key, true)
            {
                this.sock = sock;
                this.server = server;
            }

            public override void OnAuthorized()
            {
                server.OnClientAuthorized(this);
            }

            /// <summary>
            /// Called when we get disconnected
            /// </summary>
            /// <param name="reason"></param>
            public override void OnDisconnect(string reason = null)
            {
                server.OnClientDisconnected(this);
            }
        }
    }
}
