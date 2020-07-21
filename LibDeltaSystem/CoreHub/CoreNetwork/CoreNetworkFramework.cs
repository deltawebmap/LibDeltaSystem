using LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList;
using LibDeltaSystem.CoreHub.CoreNetwork.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    /// <summary>
    /// Handles core networking, acts as a server and a client. Each kind of server should create their own version of this class.
    /// </summary>
    public abstract class CoreNetworkFramework
    {
        private UdpClient listener;
        public CoreNetworkServer me;
        public DeltaConnection delta;
        public ICoreNetworkServerList list;
        private List<CoreNetworkOutgoingMessage> outgoing;
        private Thread outgoingThread;
        private uint outgoingIndex; //The outgoing message ID
        private List<ulong> previousReceivedIds; //Holds IDs that we've already seen. IDs matching will either be dropped, or we'll resend the ack for them

        public int stat_outgoingQueueSize { get { return outgoing.Count; } }
        public int stat_previousIdCount { get { return previousReceivedIds.Count; } }
        public int stat_messagesSent { get; private set; }
        public int stat_messagesReceived { get; private set; }
        public int stat_messagesReceivedDuplicate { get; private set; }
        public int stat_messagesResent { get; private set; }
        public int stat_messagesAcked { get; private set; }
        public int stat_messagesAuthFailed { get; private set; }

        public CoreNetworkFramework()
        {
            
        }

        public void Init(DeltaConnection delta, ushort my_server_id, ICoreNetworkServerList list)
        {
            //Set
            this.list = list;
            this.delta = delta;
            previousReceivedIds = new List<ulong>();
            outgoing = new List<CoreNetworkOutgoingMessage>();
            outgoingIndex = (uint)(new Random().Next(int.MinValue, int.MaxValue));

            //Find my server
            me = list.GetServerById(my_server_id);
            if (me == null)
                throw new Exception("Could not find my server ID in the server list.");

            //Verify
            if (me.id == 0)
                throw new Exception("Policy FORBIDS server IDs to be 0. This is an invalid server entry.");

            //Create listener
            listener = new UdpClient(me.port);
            _BeginListening();

            //Start sender thread
            outgoingThread = new Thread(() =>
            {
                _SendThread();
            });
            outgoingThread.IsBackground = true;
            outgoingThread.Start();
        }

        private void _SendThread()
        {
            CoreNetworkOutgoingMessage msg;
            while(true)
            {
                //Wait for the next message
                while (outgoing.Count == 0)
                    Thread.Sleep(5);

                //Try to find the first message we can send
                msg = null;
                lock(outgoing)
                {
                    foreach(var m in outgoing)
                    {
                        if(m.CanSend())
                        {
                            msg = m;
                            break;
                        }
                    }
                }

                //Abort if there was no message found
                if (msg == null)
                {
                    Thread.Sleep(10);
                    continue;
                }

                //We'll send this message
                _ForceSendMessage(msg);
            }
        }

        /// <summary>
        /// Sends a message to a server
        /// </summary>
        /// <param name="server"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        public void SendMessage(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] data)
        {
            _QueueMessage(server, opcode, data, 0);
        }

        /// <summary>
        /// Sends a message and accepts a response.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public void BeginSendMessageGetResponse(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] data, AsyncCallback callback, object asyncObject)
        {
            _QueueMessage(server, opcode, data, 0, callback, asyncObject);
        }

        public byte[] EndSendMessageGetResponse(IAsyncResult ar)
        {
            //Convert to ours
            var result = (CoreNetworkAsyncResult)ar;
            if (result._status == 0x01)
                throw new CoreNetworkRemoteErrorException(Encoding.UTF8.GetString(result._result));
            return result._result;
        }

        /// <summary>
        /// Sends a message and accepts a response.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="opcode"></param>
        /// <param name="data"></param>
        public Task<byte[]> SendMessageGetResponse(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] data)
        {
            var promise = new TaskCompletionSource<byte[]>();
            BeginSendMessageGetResponse(server, opcode, data, _AsyncSendMessageGetResponse, promise);
            return promise.Task;
        }

        private void _AsyncSendMessageGetResponse(IAsyncResult ar)
        {
            var promise = (TaskCompletionSource<byte[]>)ar.AsyncState;
            byte[] data = EndSendMessageGetResponse(ar);
            promise.SetResult(data);
        }

        private void _QueueMessage(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] data, ulong ackMessageId = 0, AsyncCallback callback = null, object asyncObject = null)
        {
            //Create and queue this outgoing message
            lock (outgoing)
            {
                outgoing.Add(new CoreNetworkOutgoingMessage
                {
                    server = server,
                    opcode = opcode,
                    payload = data,
                    lastSent = DateTime.MinValue,
                    id = _GetUniqueMessageID(),
                    ackMessageId = ackMessageId,
                    callback = callback,
                    asyncState = asyncObject
                });
            }
        }

        private uint _GetUniqueMessageID()
        {
            return outgoingIndex++;
        }

        private void _ForceSendMessage(CoreNetworkOutgoingMessage msg)
        {
            //Generate HMAC
            byte[] hmac = _GenerateHmac(me, msg.payload, msg.id, msg.opcode, 0x00, 0x00);

            //Create the payload to send, encoding it the same way as we decode it in _ListenerReceive
            byte[] outgoingPayload = new byte[32 + 4 + 2 + 2 + 2 + 2 + msg.payload.Length];
            hmac.CopyTo(outgoingPayload, 0);
            _WriteUInt32(outgoingPayload, 32, msg.id);
            _WriteUInt16(outgoingPayload, 36, me.id);
            _WriteInt16(outgoingPayload, 38, (short)msg.opcode);
            outgoingPayload[40] = 0x00;
            outgoingPayload[41] = 0x00;
            _WriteUInt16(outgoingPayload, 42, (ushort)msg.payload.Length);
            msg.payload.CopyTo(outgoingPayload, 44);

            //Send on network
            listener.Send(outgoingPayload, outgoingPayload.Length, new IPEndPoint(msg.server.address, msg.server.port));

            //Log
            delta.Log("CoreHub-CoreNetworkFramework", $"SENT message {msg.id} with opcode {msg.opcode.ToString()} ({(int)msg.opcode}) to server {msg.server.id} (type {msg.server.type.ToString()}) with payload size {msg.payload.Length} bytes", DeltaLogLevel.Debug);

            //Set stats
            msg.sendAttempts++;
            if (msg.sendAttempts == 1)
                stat_messagesSent++;
            else
                stat_messagesResent++;

            //Update
            msg.lastSent = DateTime.UtcNow;
        }

        private void _ListenerReceive(IAsyncResult ar)
        {
            //Get payload
            IPEndPoint source = null;
            byte[] data;
            try
            {
                data = listener.EndReceive(ar, ref source);
            } catch (Exception ex)
            {
                delta.Log("CoreHub-_ListenerReceive", $"CONNECTION ERROR trying to receive.", DeltaLogLevel.Medium);
                _BeginListening();
                return;
            }

            //We're going to decode the header. It follows this format:
            //SIZE  TYPE        NAME
            //256   HMAC-256    SHA-256 HMAC
            //4     UInt32      Unique (to the server) request ID
            //2     UInt16      Unique server ID
            //2     Int16       Opcode
            //1     Byte        Application version
            //1     Byte        Flags
            //2     UInt16      Content length

            //Validate that the data payload is even large enough to hold what we need
            if (data.Length < 32 + 4 + 2 + 2 + 2 + 2)
            {
                _LogConnectionSecurityError(source, "Data length not long enough to contain header.");
                _BeginListening();
                stat_messagesAuthFailed++;
                return;
            }

            //Read parts of the message
            byte[] hmac = _ReadBytes(data, 0, 32);
            uint requestId = _ReadUInt32(data, 32);
            ushort serverId = _ReadUInt16(data, 36);
            CoreNetworkOpcode opcode = (CoreNetworkOpcode)_ReadInt16(data, 38);
            byte version = data[40];
            byte flags = data[41];
            ushort payloadLength = _ReadUInt16(data, 42);
            byte[] payload = _ReadBytes(data, 44, payloadLength);

            //Validate that the data payload is even large enough to hold the content length
            if (data.Length < 32 + 4 + 2 + 2 + 2 + 2 + payloadLength)
            {
                _LogConnectionSecurityError(source, "Data length not long enough to contain payload.");
                _BeginListening();
                stat_messagesAuthFailed++;
                return;
            }

            //Find the requested server ID
            var server = list.GetServerById(serverId);
            if (server == null)
            {
                _LogConnectionSecurityError(source, "Requested server ID was not a valid server.");
                _BeginListening();
                stat_messagesAuthFailed++;
                return;
            }

            //Generate the challenge hmac
            byte[] challengeHmac = _GenerateHmac(server, payload, requestId, opcode, flags, version);
            if(!Tools.BinaryTool.CompareBytes(hmac, challengeHmac))
            {
                _LogConnectionSecurityError(source, "Challenge HMAC did not match the HMAC sent. THIS IS AN ENTRY ATTEMPT!");
                _BeginListening();
                stat_messagesAuthFailed++;
                return;
            }

            //Get the global message ID by combining the server and message IDs
            ulong globalMessageId = ((ulong)serverId << 32) | requestId;

            //We've now verified that the packet is correct!
            delta.Log("CoreHub-CoreNetworkFramework", $"GOT message {requestId} of opcode {opcode.ToString()} ({(int)opcode}) from server {server.id} (type {server.type.ToString()}) with payload {payload.Length} bytes.", DeltaLogLevel.Debug);
            stat_messagesReceived++;

            //Check if we've already gotten this message before
            if (previousReceivedIds.Contains(globalMessageId))
            {
                stat_messagesReceivedDuplicate++;

                //Search for this ID
                CoreNetworkOutgoingMessage msg = null;
                lock(outgoing)
                {
                    foreach(var m in outgoing)
                    {
                        if (m.ackMessageId == globalMessageId)
                            msg = m;
                    }
                }

                //If we found a message, resend the ACK. If we haven't, just drop the message
                if(msg != null)
                {
                    msg.ackSendRequired = true;
                    delta.Log("CoreHub-CoreNetworkFramework", $"Message {requestId} (GLOBAL {globalMessageId}) has been resent to us. Successfully found ACK and resent it.", DeltaLogLevel.Low);
                } else
                {
                    delta.Log("CoreHub-CoreNetworkFramework", $"Message {requestId} (GLOBAL {globalMessageId}) has been resent to us. ACK could not be found. It could have expired, or something has gone wrong.", DeltaLogLevel.High);
                }

                //Stop
                _BeginListening();
                return;
            } else
            {
                previousReceivedIds.Add(globalMessageId);
            }

            //Handle ACK, because it's special
            if (opcode == CoreNetworkOpcode.MESSAGE_ACK)
            {
                //Handle ACK
                _HandleAck(server, payload, flags);
            }
            else
            {
                //Handle everything else
                byte responseStatus = 0x00; //0x00-OK, 0x01-Exception hit (data will be an Exception string)
                byte[] responseData = new byte[0];
                try
                {
                    responseData = _InternalHandleRequest(server, payload, opcode, flags);
                }
                catch (Exception ex)
                {
                    responseStatus = 0x01;
                    responseData = Encoding.UTF8.GetBytes($"{ex.Message} {ex.StackTrace}");
                }

                //Create ack message
                byte[] ack = new byte[responseData.Length + 4 + 1];
                _WriteUInt32(ack, 0, requestId);
                ack[4] = responseStatus;
                responseData.CopyTo(ack, 5);

                //Send ACK message
                _QueueMessage(server, CoreNetworkOpcode.MESSAGE_ACK, ack, globalMessageId);
            }

            //Listen
            _BeginListening();
        }

        private void _BeginListening()
        {
            try
            {
                listener.BeginReceive(_ListenerReceive, null);
            } catch (Exception ex)
            {
                delta.Log("CoreHub-_BeginListening", $"BeginReceive threw an exception.", DeltaLogLevel.Medium);
                Thread.Sleep(50);
                _BeginListening();
            }
        }

        private void _LogConnectionSecurityError(IPEndPoint endpoint, string reason)
        {
            delta.Log("CoreHub-CoreNetworkFramework", $"SECURITY ERROR processing request from {endpoint.Address.ToString()}:{endpoint.Port} - {reason}", DeltaLogLevel.Alert);
        }

        private byte[] _GenerateHmac(CoreNetworkServer server, byte[] payloadData, uint requestId, CoreNetworkOpcode opcode, byte flags, byte version)
        {
            //HMACs are encoded in form...
            //SIZE  TYPE        NAME
            //8     UInt64      Server token
            //4     UInt32      Server ID
            //4     UInt32      Request ID
            //2     Int16       Opcode
            //1     Byte        Flags
            //1     Byte        Version

            //Create data to hash
            byte[] hashData = new byte[8 + 4 + 4 + 2 + 1 + 1 + payloadData.Length];
            _WriteUInt64(hashData, 0, server.token);
            _WriteUInt32(hashData, 8, server.id);
            _WriteUInt32(hashData, 12, requestId);
            _WriteInt16(hashData, 16, (short)opcode);
            hashData[18] = flags;
            hashData[19] = version;
            Array.Copy(payloadData, 0, hashData, 20, payloadData.Length);

            //Begin computing
            var hash = new HMACSHA256(BitConverter.GetBytes(server.token));
            byte[] hashBytes = hash.ComputeHash(hashData);
            if (hashBytes.Length != 32)
                throw new Exception("Unexpected hashing error.");
            return hashBytes;
        }

        private void _WriteUInt64(byte[] buffer, int index, ulong data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, index);
        }

        private void _WriteUInt32(byte[] buffer, int index, uint data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, index);
        }

        private void _WriteUInt16(byte[] buffer, int index, ushort data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, index);
        }

        private void _WriteInt16(byte[] buffer, int index, short data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, index);
        }

        private byte[] _ReadBytes(byte[] data, int index, int length)
        {
            byte[] buffer = new byte[length];
            Array.Copy(data, index, buffer, 0, length);
            return buffer;
        }

        private uint _ReadUInt32(byte[] data, int index)
        {
            return BitConverter.ToUInt32(data, index);
        }

        private ushort _ReadUInt16(byte[] data, int index)
        {
            return BitConverter.ToUInt16(data, index);
        }

        private short _ReadInt16(byte[] data, int index)
        {
            return BitConverter.ToInt16(data, index);
        }

        public void NotifyAllServerListModified(ushort ignoredClient = 0)
        {
            foreach (var s in list.GetAllServers())
            {
                if(s.id != ignoredClient)
                    SendMessage(s, CoreNetworkOpcode.MESSAGE_NOTIFY_SERVER_LIST_CHANGED, new byte[0]);
            }
        }

        private byte[] _InternalHandleRequest(CoreNetworkServer server, byte[] payload, CoreNetworkOpcode opcode, byte flags)
        {
            if (opcode == CoreNetworkOpcode.MESSAGE_PING)
                return payload;
            else if (opcode == CoreNetworkOpcode.MESSAGE_STATS)
                return _InternalHandleStats(server);
            else if (opcode == CoreNetworkOpcode.MESSAGE_NOTIFY_SERVER_LIST_CHANGED)
                return _InternalHandleNotifyServerListChanged();
            else
                return OnMessage(server, opcode, payload);
        }

        private byte[] _InternalHandleNotifyServerListChanged()
        {
            delta.Log("CoreNetworkFramework-_InternalHandleNotifyServerListChanged", $"Server list update requested. Processing...", DeltaLogLevel.Medium);
            int beforeCount = list.GetAllServers().Count;
            list.RefreshRequested();
            delta.Log("CoreNetworkFramework-_InternalHandleNotifyServerListChanged", $"Server list update finished. {list.GetAllServers().Count} servers, {list.GetAllServers().Count - beforeCount} new.", DeltaLogLevel.Medium);
            return new byte[0];
        }

        private byte[] _InternalHandleStats(CoreNetworkServer server)
        {
            //Create stats packet with this data:
            //Len   Type    Name
            //1     Byte    LibDelta version major
            //1     Byte    LibDelta version minor
            //1     Byte    Application version major
            //1     Byte    Application version minor
            //2     UShort  Server ID
            //1     Byte    Server Type
            //1     Byte    Status (should always be 0x00)
            //4     UInt32  Uptime Seconds
            //8     Int64   DateTime ticks
            //1     Byte    Operating System
            //1     Byte    Machine Name Length
            //64    Char[]  Machine Name 
            //TOTAL LENGTH: 86

            byte[] machineName = Encoding.UTF8.GetBytes(System.Environment.MachineName);

            byte[] payload = new byte[86];
            payload[0] = DeltaConnection.LIB_VERSION_MAJOR;
            payload[1] = DeltaConnection.LIB_VERSION_MINOR;
            payload[2] = delta.system_version_major;
            payload[3] = delta.system_version_minor;
            Tools.BinaryTool.WriteUInt16(payload, 4, delta.server_id);
            payload[6] = (byte)me.type;
            payload[7] = 0x00;
            Tools.BinaryTool.WriteUInt32(payload, 8, (uint)(DateTime.UtcNow - delta.start_time).TotalSeconds);
            Tools.BinaryTool.WriteInt64(payload, 12, DateTime.UtcNow.Ticks);
            payload[20] = (byte)System.Environment.OSVersion.Platform;
            payload[21] = (byte)Math.Min(64, machineName.Length);
            Array.Copy(machineName, 0, payload, 22, payload[21]);
            return payload;
        }

        public abstract byte[] OnMessage(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] payload);

        private void _HandleAck(CoreNetworkServer server, byte[] payload, byte flags)
        {
            //Decode
            uint ackMessageId = _ReadUInt32(payload, 0);
            byte status = payload[4];
            byte[] response = _ReadBytes(payload, 5, payload.Length - 5);

            //Log
            delta.Log("CoreHub-CoreNetworkFramework", $"Message ({ackMessageId}) ACK'd with status {status}", DeltaLogLevel.Debug);
            stat_messagesAcked++;

            //Find and remove the outgoing message
            CoreNetworkOutgoingMessage msg = null;
            lock (outgoing)
            {
                foreach(var m in outgoing)
                {
                    if (m.id == ackMessageId)
                    {
                        msg = m;
                        break;
                    }
                }
                if(msg == null)
                {
                    delta.Log("CoreHub-CoreNetworkFramework", $"Nessage was ACK'd, but the requested message ID ({ackMessageId}) was not found!", DeltaLogLevel.High);
                    return;
                }
                outgoing.Remove(msg);
            }

            //Run callback
            try
            {
                msg.callback?.Invoke(new CoreNetworkAsyncResult(status, response, msg.asyncState));
            } catch (Exception ex)
            {
                delta.Log("CoreHub-CoreNetworkFramework", $"Exception occurred attempting to handle ack in user's code: {ex.Message} {ex.StackTrace}", DeltaLogLevel.High);
            }

            //Remove this message from the outgoing queue for now. We might change this behavior later
            outgoing.Remove(msg);
        }
    }
}
