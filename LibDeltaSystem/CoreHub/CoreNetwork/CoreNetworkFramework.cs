﻿using LibDeltaSystem.CoreHub.CoreNetwork.CoreNetworkServerList;
using LibDeltaSystem.CoreHub.CoreNetwork.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace LibDeltaSystem.CoreHub.CoreNetwork
{
    /// <summary>
    /// Handles core networking, acts as a server and a client
    /// </summary>
    public abstract class CoreNetworkFramework
    {
        private UdpClient listener;
        private CoreNetworkServer me;
        private DeltaConnection delta;
        private ICoreNetworkServerList list;
        private List<CoreNetworkOutgoingMessage> outgoing;
        private Thread outgoingThread;
        private uint outgoingIndex; //The outgoing message ID
        private List<ulong> previousReceivedIds; //Holds IDs that we've already seen. IDs matching will either be dropped, or we'll resend the ack for them

        public CoreNetworkFramework(DeltaConnection delta, CoreNetworkServer me, ICoreNetworkServerList list)
        {
            //Set
            this.me = me;
            this.list = list;
            this.delta = delta;
            previousReceivedIds = new List<ulong>();
            outgoing = new List<CoreNetworkOutgoingMessage>();
            outgoingIndex = (uint)(new Random().Next(int.MinValue, int.MaxValue));

            //Verify
            if (me.id == 0)
                throw new Exception("Policy FORBIDS server IDs to be 0. This is an invalid server entry.");
            
            //Create listener
            listener = new UdpClient(me.port);
            listener.BeginReceive(_ListenerReceive, null);

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

            //Update
            msg.lastSent = DateTime.UtcNow;
        }

        private void _ListenerReceive(IAsyncResult ar)
        {
            //Get payload
            IPEndPoint source = null;
            byte[] data = listener.EndReceive(ar, ref source);

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
                listener.BeginReceive(_ListenerReceive, null);
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
                listener.BeginReceive(_ListenerReceive, null);
                return;
            }

            //Find the requested server ID
            var server = list.GetServerById(serverId);
            if (server == null)
            {
                _LogConnectionSecurityError(source, "Requested server ID was not a valid server.");
                listener.BeginReceive(_ListenerReceive, null);
                return;
            }

            //Generate the challenge hmac
            byte[] challengeHmac = _GenerateHmac(server, payload, requestId, opcode, flags, version);
            if(!Tools.BinaryTool.CompareBytes(hmac, challengeHmac))
            {
                _LogConnectionSecurityError(source, "Challenge HMAC did not match the HMAC sent. THIS IS AN ENTRY ATTEMPT!");
                listener.BeginReceive(_ListenerReceive, null);
                return;
            }

            //Get the global message ID by combining the server and message IDs
            ulong globalMessageId = ((ulong)serverId << 32) | requestId;

            //We've now verified that the packet is correct!
            delta.Log("CoreHub-CoreNetworkFramework", $"GOT message {requestId} of opcode {opcode.ToString()} ({(int)opcode}) from server {server.id} (type {server.type.ToString()}) with payload {payload.Length} bytes.", DeltaLogLevel.Debug);

            //Check if we've already gotten this message before
            if(previousReceivedIds.Contains(globalMessageId))
            {
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
                listener.BeginReceive(_ListenerReceive, null);
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
            listener.BeginReceive(_ListenerReceive, null);
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

        private byte[] _InternalHandleRequest(CoreNetworkServer server, byte[] payload, CoreNetworkOpcode opcode, byte flags)
        {
            if (opcode == CoreNetworkOpcode.MESSAGE_PING)
                return payload;
            else
                return OnMessage(server, opcode, payload);
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
            msg.callback?.Invoke(new CoreNetworkAsyncResult(status, response, msg.asyncState));
        }
    }
}