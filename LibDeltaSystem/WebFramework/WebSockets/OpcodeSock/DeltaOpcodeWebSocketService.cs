using LibDeltaSystem;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.OpcodeSock
{
    public delegate Task DeltaOpcodeWebSocketServiceCommandHandler(JObject payload);
    
    public abstract class DeltaOpcodeWebSocketService : DeltaWebService
    {
        public DeltaOpcodeWebSocketService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            incomingBuffer = new byte[1024];
            channel = Channel.CreateUnbounded<ISockCommand>();
            opcodeHandlers = new Dictionary<string, DeltaOpcodeWebSocketServiceCommandHandler>();
        }

        private WebSocket sock;
        private byte[] incomingBuffer;
        private Channel<ISockCommand> channel;
        private Dictionary<string, DeltaOpcodeWebSocketServiceCommandHandler> opcodeHandlers;

        public override async Task OnRequest()
        {
            //Accept WebSocket
            if (!e.WebSockets.IsWebSocketRequest)
            {
                await WriteString("Expected WebSocket request to this endpoint.", "text/plain", 400);
                return;
            }
            sock = await e.WebSockets.AcceptWebSocketAsync();

            //Send connection info
            JObject connectionInfo = new JObject();
            connectionInfo["lib_version_major"] = DeltaConnection.LIB_VERSION_MAJOR;
            connectionInfo["lib_version_minor"] = DeltaConnection.LIB_VERSION_MINOR;
            connectionInfo["app_version_major"] = conn.system_version_major;
            connectionInfo["app_version_minor"] = conn.system_version_minor;
            connectionInfo["server_instance_id"] = conn.instanceId;
            connectionInfo["server_id"] = conn.serverId;
            connectionInfo["sock_instance_id"] = _request_id;
            connectionInfo["buffer_size"] = incomingBuffer.Length;
            await SendMessage("CONNECTION_INFO", connectionInfo);

            //Add to list
            await OnSockOpened();

            //Begin get
            CancellationToken cancellationToken = new CancellationToken();
            Task<WebSocketReceiveResult> receiveTask = sock.ReceiveAsync(new ArraySegment<byte>(incomingBuffer, 0, incomingBuffer.Length), cancellationToken);
            Task<ISockCommand> commandsTask = channel.Reader.ReadAsync(cancellationToken).AsTask();

            //Loop
            string incomingTextBuffer = "";
            while (true)
            {
                //Wait for something to complete
                await Task.WhenAny(receiveTask, commandsTask);

                //Handle receiveTask
                if (receiveTask.IsCompleted)
                {
                    WebSocketReceiveResult result = receiveTask.Result;
                    if (result.CloseStatus.HasValue)
                        break;
                    if (result.MessageType != WebSocketMessageType.Text)
                        break;

                    //Write to buffer
                    incomingTextBuffer += Encoding.UTF8.GetString(incomingBuffer, 0, result.Count);

                    //Check if this is the end
                    if (result.EndOfMessage)
                    {
                        await OnSockCommandReceive(incomingTextBuffer);
                        incomingTextBuffer = "";
                    }

                    //Get next
                    receiveTask = sock.ReceiveAsync(new ArraySegment<byte>(incomingBuffer, 0, incomingBuffer.Length), cancellationToken);
                }

                //Handle commandsTask
                if (commandsTask.IsCompleted)
                {
                    await OnInternalCommandReceive(commandsTask.Result);
                    commandsTask = channel.Reader.ReadAsync(cancellationToken).AsTask();
                }
            }

            //Remove from list
            await OnSockClosed();
        }

        public abstract Task OnSockOpened();
        public abstract Task OnSockClosed();

        public virtual async Task OnCommandReceived(string opcode, JObject payload)
        {
            if (opcodeHandlers.ContainsKey(opcode))
                await opcodeHandlers[opcode](payload);
        }

        public void RegisterCommandHandler(string opcode, DeltaOpcodeWebSocketServiceCommandHandler handler)
        {
            opcodeHandlers.Add(opcode, handler);
        }

        public void EnqueueMessage(ISockCommand cmd)
        {
            channel.Writer.WriteAsync(cmd);
        }

        private async Task OnSockCommandReceive(string cmd)
        {
            //Decode
            JObject data = JsonConvert.DeserializeObject<JObject>(cmd);
            string opcode = (string)data["opcode"];
            JObject payload = (JObject)data["payload"];

            //Send
            await OnCommandReceived(opcode, payload);
        }

        private async Task OnInternalCommandReceive(ISockCommand cmd)
        {
            await cmd.HandleCommand(this);
        }

        public async Task SendMessage(string opcode, JObject payload)
        {
            //Create
            JObject p = new JObject();
            p["opcode"] = opcode;
            p["payload"] = payload;

            //Serialize
            string data = JsonConvert.SerializeObject(p);
            byte[] dataPayload = Encoding.UTF8.GetBytes(data);

            //Send
            await sock.SendAsync(new ArraySegment<byte>(dataPayload, 0, dataPayload.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public void QueueSendMessage(string opcode, JObject payload)
        {
            EnqueueMessage(new QueueMessageCommand(opcode, payload));
        }

        public static bool UtilValidateJObject(JObject o, out string error, params JObjectValidationParameter[] requiredParams)
        {
            foreach(var r in requiredParams)
            {
                if(!o.ContainsKey(r.key))
                {
                    error = $"Parameter '{r.key}' ({r.type.ToString()}) is required, but wasn't found.";
                    return false;
                }
                if(o[r.key].Type == r.type)
                {
                    error = $"Parameter '{r.key}' was expected to be of type {r.type.ToString()}, but was actually {o[r.key].Type.ToString()}.";
                    return false;
                }
            }
            error = "";
            return true;
        }

        public struct JObjectValidationParameter
        {
            public string key;
            public JTokenType type;

            public JObjectValidationParameter(string key, JTokenType type)
            {
                this.key = key;
                this.type = type;
            }
        }

        public class QueueMessageCommand : ISockCommand
        {
            private string opcode;
            private JObject payload;

            public QueueMessageCommand(string opcode, JObject payload)
            {
                this.opcode = opcode;
                this.payload = payload;
            }
            
            public async Task HandleCommand(DeltaOpcodeWebSocketService conn)
            {
                await conn.SendMessage(opcode, payload);
            }
        }
    }
}
