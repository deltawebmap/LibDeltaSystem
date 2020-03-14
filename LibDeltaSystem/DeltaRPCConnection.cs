using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LibDeltaSystem.RPC;
using Newtonsoft.Json;
using LibDeltaSystem.Db.System;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace LibDeltaSystem
{
    public class DeltaRPCConnection
    {
        private bool _connected;
        private ConcurrentQueue<byte[]> _queue;
        private ClientWebSocket _sock;
        private ulong _index = 1;
        private DeltaConnection _conn;
        private Task _reconnectTask;

        public DeltaRPCConnection(DeltaConnection conn)
        {
            _conn = conn;
            _connected = false;
            _queue = new ConcurrentQueue<byte[]>();
            _reconnectTask = _Reconnect();
        }

        private void _OnDisconnect()
        {
            _connected = false;
            if (_reconnectTask == null)
                _reconnectTask = _Reconnect();
        }

        private async Task _Reconnect()
        {
            await Task.Delay(2000);
            await _OpenConnection();
        }

        private async Task _OpenConnection()
        {
            _reconnectTask = null;
            _connected = false;

            //If old client exists, make sure it is closed
            if(_sock != null)
            {
                if(_sock.State != WebSocketState.Closed && _sock.State != WebSocketState.Aborted)
                {
                    try
                    {
                        _Log("CONNECT", "Disconnecting old client...");
                        await _sock.CloseAsync(WebSocketCloseStatus.InternalServerError, "DELTA_DISCONNECT_GENERIC", CancellationToken.None);
                    }
                    catch { }
                }
            }

            //Create client
            _sock = new ClientWebSocket();

            try
            {
                //Connect
                _Log("CONNECT", "Connecting to RPC...");
                await _sock.ConnectAsync(new Uri("ws://localhost:43281/internal/sender"), CancellationToken.None);

                //Send auth data
                _Log("CONNECT", "Sending auth data...");
                await _sock.SendAsync(_CreateAuthData(), WebSocketMessageType.Binary, true, CancellationToken.None);
                _Log("CONNECT", "Auth data sent!");

                //Send queued contents
                _connected = true;
                while (_queue.TryDequeue(out byte[] r))
                {
                    await _sock.SendAsync(r, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            } catch (Exception ex)
            {
                _Log("DISCONNECT", "Hit exception '" + ex.Message + " @ " + ex.StackTrace + "'. Reconnecting...");
                _OnDisconnect();
            }
        }

        private void _Log(string topic, string msg)
        {
            Console.WriteLine($"[RPC: {topic}] {msg}");
        }

        private async Task _SendBytes(byte[] data)
        {
            //If connected, send now. Else, queue
            try
            {
                if (_connected)
                    await _sock.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
                else
                    _queue.Enqueue(data);
            } catch (Exception ex)
            {
                _Log("DISCONNECT", "Hit exception '" + ex.Message + "'. Reconnecting...");
                _OnDisconnect();
            }
        }

        private async Task _SendMessage(ushort opcode, byte[] data)
        {
            //Create buffer to enclose this (+10 bytes)
            byte[] msg = new byte[data.Length + 10];
            BinaryTool.WriteUInt16(msg, 0, opcode);
            BinaryTool.WriteUInt64(msg, 2, _index++);
            Array.Copy(data, 0, msg, 10, data.Length);
            await _SendBytes(msg);
        }

        private async Task _SendRPCMessage(string payload, ushort filterType, byte[] filterBytes)
        {
            //Convert payload to bytes
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            //Allocate space for this
            byte[] msg = new byte[4 + payloadBytes.Length + 2 + filterBytes.Length];

            //Write the payload length and data
            BinaryTool.WriteInt32(msg, 0, payloadBytes.Length);
            Array.Copy(payloadBytes, 0, msg, 4, payloadBytes.Length);

            //Write the payload type and bytes
            BinaryTool.WriteUInt16(msg, 4 + payloadBytes.Length, filterType);
            Array.Copy(filterBytes, 0, msg, 4 + payloadBytes.Length + 2, filterBytes.Length);

            //Write
            await _SendMessage(1, msg);
        }

        private byte[] _CreateAuthData()
        {
            //Allocate space for the auth data to send
            byte[] payload = new byte[268];
            Array.Copy(_GetConnectKey(), 0, payload, 0, 256);
            payload[257] = 0x52; //r
            payload[257] = 0x50; //p
            payload[257] = 0x43; //c
            payload[257] = 0x20; //[space]
            payload[257] = 0x50; //p
            payload[257] = 0x52; //r
            payload[257] = 0x4F; //o
            payload[257] = 0x44; //d
            return payload;
        }

        /// <summary>
        /// Returns the 256-byte connection key
        /// </summary>
        /// <returns></returns>
        private byte[] _GetConnectKey()
        {
            return Convert.FromBase64String(_conn.config.rpc_key);
        }

        public async Task SendRPCMsgToUserID(RPCOpcode opcode, RPCPayload payload, ObjectId user_id)
        {
            //Create payload data
            RPCMessageContainer msg = new RPCMessageContainer
            {
                opcode = opcode,
                payload = payload,
                source = _conn.system_name,
                target_server = null
            };

            //Create filter
            byte[] filter = new byte[12];
            BinaryTool.WriteMongoID(filter, 0, user_id);

            //Send
            await _SendRPCMessage(JsonConvert.SerializeObject(msg), 0, filter);
        }

        public async Task SendRPCMsgToUserID(RPCOpcode opcode, RPCPayload payload, DbUser user)
        {
            await SendRPCMsgToUserID(opcode, payload, user._id);
        }

        public async Task SendRPCMsgToServer(RPCOpcode opcode, RPCPayload payload, ObjectId server_id)
        {
            //Create payload data
            RPCMessageContainer msg = new RPCMessageContainer
            {
                opcode = opcode,
                payload = payload,
                source = _conn.system_name,
                target_server = server_id.ToString()
            };

            //Create filter
            byte[] filter = new byte[12];
            BinaryTool.WriteMongoID(filter, 0, server_id);

            //Send
            await _SendRPCMessage(JsonConvert.SerializeObject(msg), 1, filter);
        }

        public async Task SendRPCMsgToServer(RPCOpcode opcode, RPCPayload payload, DbServer server)
        {
            await SendRPCMsgToUserID(opcode, payload, server._id);
        }

        public async Task SendRPCMsgToServerTribe(RPCOpcode opcode, RPCPayload payload, ObjectId server_id, int tribe_id)
        {
            //Create payload data
            RPCMessageContainer msg = new RPCMessageContainer
            {
                opcode = opcode,
                payload = payload,
                source = _conn.system_name,
                target_server = server_id.ToString()
            };

            //Create filter
            byte[] filter = new byte[16];
            BinaryTool.WriteMongoID(filter, 0, server_id);
            BinaryTool.WriteInt32(filter, 12, tribe_id);

            //Send
            await _SendRPCMessage(JsonConvert.SerializeObject(msg), 2, filter);
        }

        public async Task SendRPCMsgToServerTribe(RPCOpcode opcode, RPCPayload payload, DbServer server, int tribe_id)
        {
            await SendRPCMsgToServerTribe(opcode, payload, server._id, tribe_id);
        }
    }
}
