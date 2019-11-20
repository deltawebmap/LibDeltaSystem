using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LibDeltaSystem.RPC;
using Newtonsoft.Json;
using LibDeltaSystem.Db.System;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools.InternalComms
{
    /// <summary>
    /// Client for server to server communications in our backend.
    /// </summary>
    public abstract class InternalCommClient : InternalCommBase
    {
        /// <summary>
        /// Endpoint to connect to
        /// </summary>
        public IPEndPoint endpoint;

        /// <summary>
        /// Connection timeout timer
        /// </summary>
        public System.Timers.Timer connectTimeout;

        public InternalCommClient(DeltaConnection conn, byte[] key, IPEndPoint endpoint) : base(conn, key, false)
        {
            this.endpoint = endpoint;

            //Set timeout timer
            connectTimeout = new System.Timers.Timer(timeout);
            connectTimeout.AutoReset = false;
            connectTimeout.Elapsed += OnConnectFailed;
        }

        public override void OnAuthorized()
        {
            
        }

        /// <summary>
        /// Connects or reconnects to the server
        /// </summary>
        public void Connect()
        {
            //Log
            Log("Connect", "Attempting to connect...");

            //Connect
            sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            connectTimeout.Start();
            sock.BeginConnect(endpoint, OnConnect, null);
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        public override void OnDisconnect(string reason = null)
        {
            //Log
            Log("Close", "Closing connection...");

            //Close
            try
            {
                sock.Close();
            }
            catch { }

            //Reconnect
            Log("Close", "Attempting to reconnect...");
            Connect();
        }

        /// <summary>
        /// Called when a connection attempt failed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectFailed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Log
            Log("OnConnectFailed", "Connection failed. Retrying...");
            Console.WriteLine(connectTimeout.Enabled);

            //Stop timer
            connectTimeout.Stop();

            //Abort connect
            OnDisconnect();
        }

        /// <summary>
        /// Called when a connection attempt was ok.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                //Get async state
                sock.EndConnect(ar);
            }
            catch
            {
                //When we fail to connect, this will raise an error. Ignore this.
                return;
            }
            
            //Log
            Log("OnConnect", "Connection created!");

            //Stop timer
            connectTimeout.Stop();

            //Now, subscribe to listening for the message header
            BeginReceiveMessage();
        }
    }
}
