using LibDeltaSystem.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreNet.IO.Client
{
    public delegate void ClientRouterIOReceiveMessageEventArgs(RouterMessage msg);
    public delegate void ClientRouterIOClientEventArgs();

    public class ClientRouterIO : BaseRouterIO
    {
        public event ClientRouterIOReceiveMessageEventArgs OnRouterReceiveMessage;
        public event ClientRouterIOClientEventArgs OnConnected;
        public event ClientRouterIOClientEventArgs OnDisconnected;

        private IPEndPoint routerEndpoint;
        private Thread netWorkerThread;
        private Thread handlerWorkingThread;
        private ConcurrentQueue<RouterPacket> outgoingPackets;
        private ConcurrentQueue<RouterMessage> handlingMessages;

        public ClientRouterIO(IDeltaLogger logger, IRouterTransport transport, MinorMajorVersionPair libVersion, IPEndPoint routerEndpoint) : base(logger, transport, libVersion)
        {
            this.routerEndpoint = routerEndpoint;
            outgoingPackets = new ConcurrentQueue<RouterPacket>();
            handlingMessages = new ConcurrentQueue<RouterMessage>();

            //Create net worker
            netWorkerThread = new Thread(RunNetWorkerThread);
            netWorkerThread.IsBackground = true;
            netWorkerThread.Start();

            //Create handler worker
            handlerWorkingThread = new Thread(RunHandlerThread);
            handlerWorkingThread.IsBackground = true;
            handlerWorkingThread.Start();
        }

        private void RunNetWorkerThread()
        {
            RouterPacket p;
            byte[] incomingBuffer = new byte[transport.GetBufferSize()];
            byte[] outgoingBuffer = new byte[transport.GetBufferSize()];
            int incomingOffset = 0;
            Socket sock = null;
            while (true)
            {
                //Handle net code
                try
                {
                    //Open socket if needed
                    if (sock == null)
                    {
                        //Create
                        Log("NetWorker", $"Opening new socket to {routerEndpoint.ToString()}...", DeltaLogLevel.Debug);
                        sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        sock.Connect(routerEndpoint);
                        Log("NetWorker", $"Successfully opened socket.", DeltaLogLevel.Debug);

                        //Send event
                        OnConnected?.Invoke();
                    }

                    //Check if the socket is still connected
                    if (!sock.Connected)
                        throw new Exception("Socket was disconnected.");

                    //Try to access the next packet 
                    if (outgoingPackets.TryDequeue(out p))
                    {
                        //Serialize
                        int len = transport.EncodePacket(outgoingBuffer, p);

                        //Send
                        sock.Send(outgoingBuffer, len, SocketFlags.None);
                    }

                    //Try to get the next packet
                    if (sock.Available > 0)
                    {
                        //Read
                        int read = sock.Receive(incomingBuffer, incomingOffset, incomingBuffer.Length - incomingOffset, SocketFlags.None);

                        //Decode
                        int consumed = transport.DecodePacket(incomingBuffer, out p);
                        int notConsumed = read - consumed;

                        //Shift
                        for (var i = 0; i < notConsumed; i++)
                            incomingBuffer[i] = incomingBuffer[i + consumed];
                        incomingOffset = notConsumed;

                        //Handle
                        _OnReceivePacket(p);
                    }
                }
                catch (Exception ex)
                {
                    //Disconnect and try again
                    Log("NetWorker", $"Disconnected from router! '{ex.Message}' Attempting reconnection shortly...", DeltaLogLevel.Medium);
                    try
                    {
                        sock.Close();
                        sock.Dispose();
                    }
                    catch { }
                    sock = null;
                    OnDisconnected?.Invoke();
                    Thread.Sleep(10000);
                }

                //Wait
                Thread.Sleep(2);
            }
        }

        private void RunHandlerThread()
        {
            RouterMessage msg;
            while(true)
            {
                //Dequeue
                while (!handlingMessages.TryDequeue(out msg))
                    Thread.Sleep(2);

                //Process
                OnRouterReceiveMessage?.Invoke(msg);
            }
        }

        public override void RouterReceiveMessage(RouterMessage msg)
        {
            handlingMessages.Enqueue(msg);
        }

        public override void QueueOutgoingMessage(RouterPacket packet)
        {
            outgoingPackets.Enqueue(packet);
        }
    }
}
