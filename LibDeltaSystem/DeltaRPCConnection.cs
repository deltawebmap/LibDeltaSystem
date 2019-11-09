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

namespace LibDeltaSystem
{
    public class DeltaRPCConnection
    {
        /// <summary>
        /// Connection to the server
        /// </summary>
        public Socket sock;

        /// <summary>
        /// Delta connection
        /// </summary>
        public DeltaConnection conn;

        /// <summary>
        /// Salt, as sent by the server
        /// </summary>
        public byte[] salt;

        /// <summary>
        /// Authentication key
        /// </summary>
        public byte[] key;

        /// <summary>
        /// The outbound message queue
        /// </summary>
        public ConcurrentQueue<Tuple<int, byte[][]>> outboundQueue = new ConcurrentQueue<Tuple<int, byte[][]>>();

        public DeltaRPCConnection(DeltaConnection conn)
        {
            this.conn = conn;
            this.key = Convert.FromBase64String(conn.config.rpc_key);
        }

        /// <summary>
        /// Starts the connection
        /// </summary>
        public void Init()
        {
            Thread t = new Thread(() =>
            {
                Connect();
                while (true)
                {
                    //Try to send the message
                    Tuple<int, byte[][]> data = null;
                    try
                    {
                        //Keep dequeuing messages
                        while (outboundQueue.TryDequeue(out data))
                        {
                            //Send
                            SendRawPacket(data.Item1, data.Item2);
                        }

                        //Delay
                        Thread.Sleep(10);
                    }
                    catch
                    {
                        Console.WriteLine("Disconnected!");
                        //Failed. Put this message back in
                        if(data != null)
                            outboundQueue.Enqueue(data);

                        //Reconnect and retry
                        Thread.Sleep(5000);
                        Connect();
                    }
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Sends an RPC message
        /// </summary>
        public void SendRPCMessage(RPCOpcode opcode, string target_server_id, RPCPayload payload, RPCFilter filter)
        {
            //Create the actual payload message
            byte[] message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RPCMessageContainer
            {
                opcode = opcode,
                target_server = target_server_id,
                payload = payload,
                source = conn.system_name
            }));

            //Encode the filter
            byte[] filterMsg = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(filter));

            //Queue
            QueueMessage(1, filterMsg, message);
        }

        /// <summary>
        /// Sends a message to all users in a tribe
        /// </summary>
        /// <param name="opcode">Message opcode</param>
        /// <param name="payload">Message payload</param>
        /// <param name="server">Server</param>
        /// <param name="tribeId">Tribe ID</param>
        /// <returns></returns>
        public void SendRPCMessageToTribe(RPCOpcode opcode, RPCPayload payload, DbServer server, int tribeId)
        {
            //Create filter to use
            RPCFilter filter = new RPCFilter
            {
                type = "TRIBE",
                keys = new Dictionary<string, string>
                {
                    {"TRIBE_ID", tribeId.ToString() },
                    {"SERVER_ID", server.id }
                }
            };

            //Send
            SendRPCMessage(opcode, server.id, payload, filter);
        }

        /// <summary>
        /// Sends a message to all users on a server
        /// </summary>
        /// <param name="opcode">Message opcode</param>
        /// <param name="payload">Message payload</param>
        /// <param name="server">Server</param>
        /// <returns></returns>
        public void SendRPCMessageToServer(RPCOpcode opcode, RPCPayload payload, DbServer server)
        {
            //Create filter to use
            RPCFilter filter = new RPCFilter
            {
                type = "SERVER",
                keys = new Dictionary<string, string>
                {
                    {"SERVER_ID", server.id }
                }
            };

            //Send
            SendRPCMessage(opcode, server.id, payload, filter);
        }

        /// <summary>
        /// Sends a message to a user
        /// </summary>
        /// <param name="opcode">Message opcode</param>
        /// <param name="payload">Message payload</param>
        /// <param name="user">User</param>
        /// <returns></returns>
        public void SendRPCMessageToUser(RPCOpcode opcode, RPCPayload payload, string user_id)
        {
            //Create filter to use
            RPCFilter filter = new RPCFilter
            {
                type = "USER_ID",
                keys = new Dictionary<string, string>
                {
                    {"USER_ID", user_id },
                }
            };

            //Send
            SendRPCMessage(opcode, null, payload, filter);
        }

        /// <summary>
        /// Sends a message to a user
        /// </summary>
        /// <param name="opcode">Message opcode</param>
        /// <param name="payload">Message payload</param>
        /// <param name="user">User</param>
        /// <returns></returns>
        public void SendRPCMessageToUser(RPCOpcode opcode, RPCPayload payload, DbUser user)
        {
            SendRPCMessageToUser(opcode, payload, user.id);
        }

        /// <summary>
        /// Adds a message to the queue
        /// </summary>
        private void QueueMessage(int opcode, params byte[][] data)
        {
            outboundQueue.Enqueue(new Tuple<int, byte[][]>(opcode, data));
        }

        /// <summary>
        /// Connects to the system
        /// </summary>
        private bool Connect()
        {
            //If we already have a connection, end it
            if(sock != null)
            {
                Console.WriteLine("RECONNECTING!");

                //Try to close it
                try
                {
                    sock.Close();
                }
                catch { }

                //Wait
                Thread.Sleep(5000);
            }

            try
            {
                //Create a new connection
                sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(new IPEndPoint(IPAddress.Parse(conn.config.rpc_ip), conn.config.rpc_port));

                //Get the salt, it is always the first thing downloaded
                Thread.Sleep(500);
                salt = new byte[32];
                sock.Receive(salt);

                //Now we'll authenticate by sending back our key, encoded with an HMAC
                //sock.Send(HMACTool.ComputeHMAC(key, salt, key));
                return true;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Writes and Int32 to a buffer
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        private void HelperWriteInt32(byte[] buf, int pos, int data)
        {
            byte[] d = BitConverter.GetBytes(data);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(d);
            Array.Copy(d, 0, buf, pos, 4);
        }

        /// <summary>
        /// Sends a raw encoded packet
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="parts"></param>
        private void SendRawPacket(int opcode, params byte[][] parts)
        {
            //Allocate space
            int len = 32 + 4 + 4 + 4;
            foreach (var p in parts)
                len += 4 + p.Length;

            //Create buffer
            byte[] buffer = new byte[len];

            //Write length
            HelperWriteInt32(buffer, 0, len-4);

            //Set opcode
            int offset = 36;
            HelperWriteInt32(buffer, offset, opcode);
            offset += 4;

            //Set number of parts
            HelperWriteInt32(buffer, offset, parts.Length);
            offset += 4;

            //Set parts
            for (int i = 0; i<parts.Length; i+=1)
            {
                //Write length
                HelperWriteInt32(buffer, offset, parts[i].Length);
                offset += 4;

                //Write content
                Array.Copy(parts[i], 0, buffer, offset, parts[i].Length);
                offset += parts[i].Length;
            }

            //Calculate and set HMAC
            byte[] hmac = HMACTool.ComputeHMAC(key, salt, key, buffer);
            Array.Copy(hmac, 0, buffer, 4, 32);

            //Send
            sock.Send(buffer);
        }
    }
}
