using LibDeltaSystem.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LibDeltaSystem.CoreNet.IO.Server
{
    public delegate void RouterServerClientEventArgs<T>(T session);
    public delegate void RouterServerClientMessageEventArgs<T>(T session, RouterMessage msg);
    public delegate T RouterServerMakeClient<T>(IServerRouterIO server, Socket sock);

    public class ServerRouterIO<T> : IServerRouterIO where T: ServerRouterSession
    {
        public IDeltaLogger baseLogger;
        public IRouterTransport transport;
        public MinorMajorVersionPair appVersion;
        private Socket serverSock;
        private List<T> connectedClients;
        private ConcurrentQueue<Tuple<T, RouterPacket>> queuedOutgoingPackets;
        private Thread workerThread;
        private RouterServerMakeClient<T> funcConstructClient;

        public event RouterServerClientEventArgs<T> OnClientConnected;
        public event RouterServerClientEventArgs<T> OnClientDropped;
        public event RouterServerClientMessageEventArgs<T> OnClientMessage;

        public ServerRouterIO(IDeltaLogger baseLogger, IRouterTransport transport, MinorMajorVersionPair appVersion, IPEndPoint bindAddr, RouterServerMakeClient<T> funcConstructClient)
        {
            this.baseLogger = baseLogger;
            this.transport = transport;
            this.appVersion = appVersion;
            this.funcConstructClient = funcConstructClient;
            connectedClients = new List<T>();
            queuedOutgoingPackets = new ConcurrentQueue<Tuple<T, RouterPacket>>();

            //Spin up worker therad
            workerThread = new Thread(OutgoingQueueWorkerThread);
            workerThread.IsBackground = true;
            workerThread.Start();

            //Open server
            serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(bindAddr);
            serverSock.Listen(10);
            serverSock.BeginAccept(ClientAccepted, null);
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            //Try to accept
            try
            {
                //Open
                var clientSock = serverSock.EndAccept(ar);

                //Create client
                var client = funcConstructClient(this, clientSock);

                //Add to clients
                lock (connectedClients)
                    connectedClients.Add(client);
                OnClientConnected?.Invoke(client);

                //Listen
                client.ListenIncoming();
            }
            catch (Exception ex)
            {
                Log("ClientAccepted", "Unexpected error accepting client: " + ex.Message + ex.StackTrace, DeltaLogLevel.High);
            }

            //Listen
            serverSock.BeginAccept(ClientAccepted, null);
        }

        public void OnIncomingClientMessage(ServerRouterSession client, RouterMessage msg)
        {
            OnClientMessage?.Invoke((T)client, msg);
        }

        public void SendClientPacket(ServerRouterSession client, RouterPacket msg)
        {
            queuedOutgoingPackets.Enqueue(new Tuple<T, RouterPacket>((T)client, msg));
        }

        public void DropClient(ServerRouterSession clientIn)
        {
            T client = (T)clientIn;
            bool exists;
            lock (connectedClients)
            {
                exists = connectedClients.Contains(client);
                if (exists)
                    connectedClients.Remove(client);
            }
            if (exists)
            {
                OnClientDropped?.Invoke(client);
                try
                {
                    client.sock.Close();
                    client.sock.Dispose();
                }
                catch { }
            }
        }

        private void OutgoingQueueWorkerThread()
        {
            byte[] buffer = new byte[BaseRouterIO.MESSAGE_TOTAL_SIZE];
            Tuple<T, RouterPacket> p;
            while (true)
            {
                //Get next element
                while (!queuedOutgoingPackets.TryDequeue(out p))
                    Thread.Sleep(2);

                //Serialize
                int len = transport.EncodePacket(buffer, p.Item2);

                //Send
                try
                {
                    p.Item1.sock.Send(buffer, len, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Log("OutgoingQueueWorkerThread", "Communication error with client. Dropping client and further messages to client...", DeltaLogLevel.Medium);
                    DropClient(p.Item1);
                }
            }
        }

        public void Log(string topic, string message, DeltaLogLevel level)
        {
            baseLogger.Log("RouterServerIO-" + topic, message, level);
        }

        public MinorMajorVersionPair GetAppVersion()
        {
            return appVersion;
        }

        public IRouterTransport GetTransport()
        {
            return transport;
        }
    }
}
