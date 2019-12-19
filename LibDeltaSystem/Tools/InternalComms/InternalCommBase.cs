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
    public delegate Task SubscribedMessage(Dictionary<string, byte[]> data);

    public abstract class InternalCommBase
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
        /// Set to true when the salt is set
        /// </summary>
        public bool salt_valid;

        /// <summary>
        /// Authentication key
        /// </summary>
        public byte[] key;

        /// <summary>
        /// The queue is suspended while this is false
        /// </summary>
        public bool authenticated = false;

        /// <summary>
        /// Timeout in ms
        /// </summary>
        public int timeout = 2000;

        /// <summary>
        /// Size of the message header
        /// </summary>
        public const int HEADER_SIZE = 40;

        /// <summary>
        /// Size of the chunk header
        /// </summary>
        public const int CHUNK_HEADER_SIZE = 40;

        /// <summary>
        /// String sent in the auth request. Varies from version
        /// </summary>
        public const string WELCOME_STRING = "(C) DeltaWebMap, RomanPort 2019 - Protocol Version 0";

        /// <summary>
        /// Universal buffer for receive
        /// </summary>
        public byte[] receive_buffer;

        /// <summary>
        /// Buffer for recieving chunks
        /// </summary>
        public Dictionary<string, byte[]> receive_chunk_buffer;

        /// <summary>
        /// Buffer for recieving chunks
        /// </summary>
        public int receive_chunk_opcode;

        /// <summary>
        /// Name of the connected party
        /// </summary>
        public string connected_party_name;

        /// <summary>
        /// True if this is the server
        /// </summary>
        public bool is_server;

        /// <summary>
        /// The outbound message queue for messages sent before we're authenticated (or if sent when the connection was closed)
        /// </summary>
        public ConcurrentQueue<Tuple<int, Dictionary<string, byte[]>>> authOutgoingQueue;

        /// <summary>
        /// Messages that we got before auth was completed. Sent when we authenticate
        /// </summary>
        public ConcurrentQueue<Tuple<int, Dictionary<string, byte[]>>> authIncomingQueue;

        /// <summary>
        /// Thread that processes in the background
        /// </summary>
        private Thread sendThread;

        public InternalCommBase(DeltaConnection conn, byte[] key, bool is_server)
        {
            this.conn = conn;
            this.key = key;
            this.salt = new byte[32];
            this.receive_chunk_buffer = new Dictionary<string, byte[]>();
            this.is_server = is_server;
            this.authIncomingQueue = new ConcurrentQueue<Tuple<int, Dictionary<string, byte[]>>>();
            this.authOutgoingQueue = new ConcurrentQueue<Tuple<int, Dictionary<string, byte[]>>>();
        }

        /// <summary>
        /// Logs
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        public void Log(string topic, string msg)
        {
            if(conn.debug_mode)
                Console.WriteLine($"[{topic}] -> {msg}");
        }

        /// <summary>
        /// Sets the salt to a usable value
        /// </summary>
        /// <param name="salt"></param>
        public void SetSalt(byte[] salt)
        {
            this.salt = salt;
            salt_valid = true;
            Log("SetSalt", "Set salt to " + Convert.ToBase64String(salt));
        }

        /// <summary>
        /// Called when the socket is disconnected
        /// </summary>
        /// <param name="reason"></param>
        public abstract void OnDisconnect(string reason = null);

        public void InternalOnDisconnect(string reason = null)
        {
            //Log
            Log("InternalOnDisconnect", "Reason: " + reason);

            //Set flags
            authenticated = false;
            salt_valid = false;

            //Send events
            OnDisconnect(reason);
        }

        /// <summary>
        /// Subscribes to listening for header data
        /// </summary>
        public void BeginReceiveMessage()
        {
            //Log
            Log("BeginReceiveMessage", "Starting to wait for header data.");

            //Create a buffer for header contents
            receive_buffer = new byte[HEADER_SIZE];

            //Subscribe to messages
            sock.BeginReceive(receive_buffer, 0, HEADER_SIZE, SocketFlags.None, OnReceiveMessageHeader, 0);
        }

        /// <summary>
        /// Called when we download header message data
        /// </summary>
        /// <param name="ar"></param>
        public void OnReceiveMessageHeader(IAsyncResult ar)
        {
            try
            {
                //Get contents
                int received = sock.EndReceive(ar);
                int offset = (int)ar.AsyncState + received;

                //Log
                Log("OnReceiveMessageHeader", $"Received Message Header; Got {received} bytes, {offset}/{receive_buffer.Length} bytes; Connected: {sock.Connected}");

                //If we received 0, stop
                if(received == 0)
                    InternalOnDisconnect("Received 0 bytes; closing connection!");

                //Check if we need to download more data
                if (offset < receive_buffer.Length)
                {
                    sock.BeginReceive(receive_buffer, offset, receive_buffer.Length - offset, SocketFlags.None, OnReceiveMessageHeader, offset);
                    return;
                }

                //Read HMAC
                byte[] hmac = new byte[32];
                Array.Copy(receive_buffer, 0, hmac, 0, 32);

                //Calculate HMAC and compare it 
                CheckHMAC(hmac, receive_buffer, receive_buffer.Length - 32, 32);

                //Read ints
                int opcode = BinaryTool.ReadInt32(receive_buffer, 32);
                int firstChunkLength = BinaryTool.ReadInt32(receive_buffer, 36);

                //Now, download the next chunk
                receive_chunk_buffer.Clear();
                receive_chunk_opcode = opcode;
                receive_buffer = new byte[firstChunkLength + CHUNK_HEADER_SIZE];
                sock.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, OnReceiveMessageChunk, 0);
            } catch (Exception ex)
            {
                InternalOnDisconnect(ex.Message);
            }
        }

        /// <summary>
        /// Calculates an HMAC and compares it. Throws an exception if it is incorrect
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public void CheckHMAC(byte[] sentHMAC, byte[] data, int length, int offset = 0)
        {
            byte[] intendedHMAC = CalculateHMAC(data, length, offset);
            if (!HMACTool.CompareHMAC(sentHMAC, intendedHMAC))
                throw new Exception("HMAC authorization failed!");
        }
        
        /// <summary>
        /// Calculates an HMAC.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] CalculateHMAC(byte[] data, int length, int offset = 0)
        {
            //If the salt is not set, do not write an HMAC
            if (!salt_valid)
                return new byte[32];
            return HMACTool.ComputeHMAC(key, salt, data, length, offset);
        }

        /// <summary>
        /// Called when we download a chunk
        /// </summary>
        /// <param name="ar"></param>
        public void OnReceiveMessageChunk(IAsyncResult ar)
        {
            try
            {
                //Get contents
                int received = sock.EndReceive(ar);
                int offset = (int)ar.AsyncState + received;

                //Log
                Log("OnReceiveMessageChunk", $"Received Message Chunk; Got {received} bytes, {offset}/{receive_buffer.Length} bytes");

                //If we received 0, stop
                if (received == 0)
                {
                    InternalOnDisconnect("Received 0 bytes; closing connection!");
                    return;
                }

                //Check if we need to download more data
                if (offset < receive_buffer.Length)
                {
                    sock.BeginReceive(receive_buffer, offset, receive_buffer.Length - offset, SocketFlags.None, OnReceiveMessageChunk, offset);
                    return;
                }

                //Read hmac and name length
                byte[] hmac = new byte[32];
                Array.Copy(receive_buffer, 0, hmac, 0, 32);
                int nameLength = BinaryTool.ReadInt32(receive_buffer, 32);

                //Read name
                byte[] nameBinary = new byte[nameLength];
                Array.Copy(receive_buffer, 36, nameBinary, 0, nameLength);

                //Read payload
                byte[] payload = new byte[receive_buffer.Length - CHUNK_HEADER_SIZE - nameLength];
                Array.Copy(receive_buffer, 36 + nameLength, payload, 0, payload.Length);

                //Get next chunk length
                int nextChunkLength = BinaryTool.ReadInt32(receive_buffer, receive_buffer.Length - 4);

                //Verify that the HMAC is ok
                CheckHMAC(hmac, receive_buffer, receive_buffer.Length - 32, 32);

                //Add to the list of values
                receive_chunk_buffer.Add(Encoding.ASCII.GetString(nameBinary), payload);

                //If the next is -1, this is the last one
                if (nextChunkLength == -1)
                    OnMessageReceived();
                else
                {
                    receive_buffer = new byte[nextChunkLength + CHUNK_HEADER_SIZE];
                    sock.BeginReceive(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, OnReceiveMessageChunk, 0);
                }
            } catch (Exception ex)
            {
                InternalOnDisconnect(ex.Message);
            }
        }

        /// <summary>
        /// Called when we have downloaded an entire message
        /// </summary>
        public void OnMessageReceived()
        {
            //Log
            Log("OnMessageReceived", $"Got message with opcode {receive_chunk_opcode}.");

            //Handle
            if (receive_chunk_opcode < 0)
            {
                //Handle special opcode
                if(receive_chunk_opcode == -1)
                {
                    //This is a set salt request. This runs on the client side
                    //Check to make sure that we only set an unset salt
                    if (salt_valid)
                        throw new Exception("You may not change the salt once it is set. This is a violation of policy and it is likely that this is an attacker!");

                    //Set the salt used
                    SetSalt(receive_chunk_buffer["SALT"]);

                    //Send an auth message to the other party
                    RawSendMessage(-2, new Dictionary<string, byte[]>
                    {
                        {"CLIENT_NAME", Encoding.UTF8.GetBytes(conn.system_name) },
                        {"VERIFICATION", HMACTool.ComputeHMAC(key, salt, Encoding.ASCII.GetBytes(WELCOME_STRING)) }
                    });
                }
                if(receive_chunk_opcode == -2)
                {
                    //This is an auth request. Verify and set flag. This runs on the server side
                    //Verify that we have set a salt, but we are not trusted yet
                    if (!salt_valid || authenticated)
                        throw new Exception("Both clients must create a shared salt and must not be authenticated before running this command.");

                    //Check that we are a server
                    if (!is_server)
                        throw new Exception("This command can only be run from the server.");

                    //Generate an HMAC and check
                    if (!HMACTool.CompareHMAC(receive_chunk_buffer["VERIFICATION"], HMACTool.ComputeHMAC(key, salt, Encoding.ASCII.GetBytes(WELCOME_STRING))))
                        throw new Exception("HMAC sent did not match the intended HMAC.");

                    //We trust the client. Send the client a message so that they can trust us
                    InternalOnAuthorized();
                    connected_party_name = Encoding.UTF8.GetString(receive_chunk_buffer["CLIENT_NAME"]);
                    RawSendMessage(-3, new Dictionary<string, byte[]>
                    {
                        {"CLIENT_NAME", Encoding.UTF8.GetBytes(conn.system_name) },
                        {"VERIFICATION", HMACTool.ComputeHMAC(key, salt, Encoding.ASCII.GetBytes(WELCOME_STRING), new byte[16]) }
                    });

                    //Log
                    Log("OnMessageReceived", $"Client party authorized correctly as {connected_party_name}!");
                }
                if(receive_chunk_opcode == -3)
                {
                    //This is an auth request from the server to the client.
                    //Verify that we have set a salt, but we are not trusted yet
                    if (!salt_valid || authenticated)
                        throw new Exception("Both clients must create a shared salt and must not be authenticated before running this command.");

                    //Check that we are a client
                    if (is_server)
                        throw new Exception("This command can only be run from the client.");

                    //Generate an HMAC and check
                    if (!HMACTool.CompareHMAC(receive_chunk_buffer["VERIFICATION"], HMACTool.ComputeHMAC(key, salt, Encoding.ASCII.GetBytes(WELCOME_STRING), new byte[16])))
                        throw new Exception("HMAC sent did not match the intended HMAC.");

                    //We trust the client
                    InternalOnAuthorized();
                    connected_party_name = Encoding.UTF8.GetString(receive_chunk_buffer["CLIENT_NAME"]);

                    //Log
                    Log("OnMessageReceived", $"Server party authorized correctly as {connected_party_name}!");
                }
            } else
            {
                //Make sure that we are authenticated before sending this
                if (authenticated)
                    HandleMessage(receive_chunk_opcode, new Dictionary<string, byte[]>(receive_chunk_buffer));
                else
                    authIncomingQueue.Enqueue(new Tuple<int, Dictionary<string, byte[]>>(receive_chunk_opcode, new Dictionary<string, byte[]>(receive_chunk_buffer)));
            }

            //Listen for next message
            BeginReceiveMessage();
        }

        /// <summary>
        /// Called when we get a user-handled message
        /// </summary>
        /// <param name="opcode">The opcode used</param>
        /// <param name="payloads">The data sent</param>
        /// <returns></returns>
        public abstract Task HandleMessage(int opcode, Dictionary<string, byte[]> payloads);

        /// <summary>
        /// Called internally when we are authorized
        /// </summary>
        private void InternalOnAuthorized()
        {
            //Set flags
            authenticated = true;

            //Empty outgoing queue
            Tuple<int, Dictionary<string, byte[]>> data;
            while (authIncomingQueue.TryDequeue(out data))
            {
                HandleMessage(data.Item1, data.Item2).GetAwaiter().GetResult();
            }

            //Empty incoming queue
            while (authOutgoingQueue.TryDequeue(out data))
            {
                RawSendMessage(data.Item1, data.Item2);
            }

            //Call next on authorized
            OnAuthorized();
        }

        /// <summary>
        /// Called when we have been authorized
        /// </summary>
        public abstract void OnAuthorized();

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="payloads"></param>
        public void RawSendMessage(int opcode, Dictionary<string, byte[]> payloads)
        {
            //If we're not authenticated yet, add this to the queue
            if(!authenticated && opcode >= 0)
            {
                authOutgoingQueue.Enqueue(new Tuple<int, Dictionary<string, byte[]>>(opcode, payloads));
                return;
            }

            try
            {
                //Allocate space for the message
                int length = HEADER_SIZE;
                List<Tuple<string, byte[]>> payloadsList = new List<Tuple<string, byte[]>>();
                foreach (var p in payloads)
                {
                    length += CHUNK_HEADER_SIZE + Encoding.ASCII.GetByteCount(p.Key) + p.Value.Length;
                    payloadsList.Add(new Tuple<string, byte[]>(p.Key, p.Value));
                }
                byte[] buffer = new byte[length];

                //If there are no payloads, throw an error
                if (payloadsList.Count == 0)
                    throw new Exception("One or more payloads are required!");

                //Write header data
                BinaryTool.WriteInt32(buffer, 32, opcode);
                BinaryTool.WriteInt32(buffer, 36, Encoding.ASCII.GetByteCount(payloadsList[0].Item1) + payloadsList[0].Item2.Length);

                //Calculate HMAC of the header and set it
                byte[] hmac = CalculateHMAC(buffer, HEADER_SIZE - 32, 32);
                Array.Copy(hmac, 0, buffer, 0, 32);

                //Write each chunk
                int offset = HEADER_SIZE;
                for (int i = 0; i < payloadsList.Count; i++)
                {
                    //Save position of HMAC
                    int hmacPos = offset;

                    //Skip HMAC and write content, starting with name length
                    offset += 32;
                    int nameLength = Encoding.ASCII.GetByteCount(payloadsList[i].Item1);
                    BinaryTool.WriteInt32(buffer, offset, nameLength);
                    offset += 4;

                    //Write chunk name and chunk payload
                    Array.Copy(Encoding.ASCII.GetBytes(payloadsList[i].Item1), 0, buffer, offset, nameLength);
                    offset += nameLength;

                    //Write payload
                    Array.Copy(payloadsList[i].Item2, 0, buffer, offset, payloadsList[i].Item2.Length);
                    offset += payloadsList[i].Item2.Length;

                    //Now, write -1 if there are no more payloads, or the length of the payload if there is another
                    if (payloadsList.Count == i + 1)
                        BinaryTool.WriteInt32(buffer, offset, -1);
                    else
                        BinaryTool.WriteInt32(buffer, offset, Encoding.ASCII.GetByteCount(payloadsList[i + 1].Item1) + payloadsList[i + 1].Item2.Length);
                    offset += 4;

                    //Calculate the HMAC of the data
                    Array.Copy(CalculateHMAC(buffer, offset - hmacPos - 32, hmacPos + 32), 0, buffer, hmacPos, 32);
                }

                //Now, send this
                sock.Send(buffer);
            } catch (Exception ex)
            {
                InternalOnDisconnect(ex.Message);
            }
        }

        /// <summary>
        /// Helper for reading data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payloads"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetJsonFromPayload<T>(Dictionary<string, byte[]> payloads, string name)
        {
            //First, get string data
            string data = Encoding.UTF8.GetString(payloads[name]);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
