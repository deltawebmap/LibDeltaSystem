using LibDeltaSystem.CoreNet.IO.Entities;
using LibDeltaSystem.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreNet.IO
{
    public abstract class BaseRouterIO : IDeltaLogger
    {
        public const int MESSAGE_PAYLOAD_SIZE = 512;
        public const int MESSAGE_TOTAL_SIZE = MESSAGE_PAYLOAD_SIZE + 32;

        private IDeltaLogger baseLogger;
        public IRouterTransport transport;
        private int outgoingMessageId;
        private MinorMajorVersionPair appVersion;
        private Dictionary<ulong, RouterMessage> receivingPackets;
        private ConcurrentDictionary<int, Channel<RouterMessage>> waitingResponseSessions;

        public BaseRouterIO(IDeltaLogger baseLogger, IRouterTransport transport, MinorMajorVersionPair appVersion)
        {
            this.baseLogger = baseLogger;
            this.transport = transport;
            this.appVersion = appVersion;
            outgoingMessageId = new Random().Next(1, int.MaxValue - 1);
            receivingPackets = new Dictionary<ulong, RouterMessage>();
            waitingResponseSessions = new ConcurrentDictionary<int, Channel<RouterMessage>>();
        }

        public Task<IoPingResponse> SendPing()
        {
            return SendPing(CancellationToken.None);
        }

        public async Task<IoPingResponse?> SendPing(int timeoutMs)
        {
            var task = SendPing();
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
            {
                return task.Result;
            }
            else
            {
                return null;
            }
        }

        public async Task<IoPingResponse> SendPing(CancellationToken token)
        {
            var channel = SendSystemMessageGetResponseChannel(-1, new byte[0]);
            var r = await channel.ReadAsync(token).AsTask();
            return new IoPingResponse
            {
                lib_version_major = r.payload[0],
                lib_version_minor = r.payload[1],
                app_version_major = r.payload[2],
                app_version_minor = r.payload[3]
            };
        }

        public void SendMessage(short opcode, byte[] payload)
        {
            _SendMessage(opcode, payload, 0, 0);
        }

        public void SendMessageAsResponse(short opcode, byte[] payload, int responseToken, bool isEnd)
        {
            //Make flags
            int flags = isEnd ? 3 : 1;

            //Send
            _SendMessage(opcode, payload, (short)flags, responseToken);
        }

        public ChannelReader<RouterMessage> SendMessageGetResponseChannelSerialized<T>(short opcode, T payload)
        {
            return SendMessageGetResponseChannel(opcode, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
        }

        private ChannelReader<RouterMessage> SendSystemMessageGetResponseChannel(short opcode, byte[] payload)
        {
            return SendMessageGetResponseChannel(opcode, payload, 0b00000010);
        }

        public ChannelReader<RouterMessage> SendMessageGetResponseChannel(short opcode, byte[] payload)
        {
            return SendMessageGetResponseChannel(opcode, payload, 0);
        }

        private ChannelReader<RouterMessage> SendMessageGetResponseChannel(short opcode, byte[] payload, short flags)
        {
            //Get a response token
            int token = GetUniqueMessageId();

            //Create reader to use
            var channel = Channel.CreateBounded<RouterMessage>(255);

            //Register
            waitingResponseSessions.TryAdd(token, channel);

            //Send the message
            _SendMessage(opcode, payload, flags, token);

            //Return channel
            return channel.Reader;
        }

        private void _SendMessage(short opcode, byte[] payload, short flags, int responseToken)
        {
            //Generate a unique message ID
            int messageId = GetUniqueMessageId();

            //We're going to split this up into packets that we can send
            int packetsRequired = GetChunksInPayload(payload.Length);
            for (int i = 0; i < packetsRequired; i++)
            {
                //Create buffer
                int chunkSize = Math.Min(MESSAGE_PAYLOAD_SIZE, payload.Length - (i * MESSAGE_PAYLOAD_SIZE));
                byte[] chunkBuffer = new byte[chunkSize];
                Array.Copy(payload, i * MESSAGE_PAYLOAD_SIZE, chunkBuffer, 0, chunkSize);

                //Send
                QueueOutgoingMessage(new RouterPacket
                {
                    flags = flags,
                    packet_payload_length = (short)chunkSize,
                    total_message_length = payload.Length,
                    chunk_index = (short)i,
                    opcode = opcode,
                    sender_addr_local = 0,
                    sender_addr_router = 0,
                    lib_version_major = DeltaConnection.LIB_VERSION_MAJOR,
                    lib_version_minor = DeltaConnection.LIB_VERSION_MINOR,
                    app_version_major = appVersion.major,
                    app_version_minor = appVersion.minor,
                    message_id = messageId,
                    response_token = responseToken,
                    payload = chunkBuffer
                });
            }
        }

        private void HandleIncomingMessage(RouterMessage msg, RouterPacket lastPacket)
        {
            if (lastPacket.CheckFlag(0))
            {
                //This is a response. The response token will be registered and waiting by us. We'll just have to respond to it
                if (waitingResponseSessions.TryGetValue(lastPacket.response_token, out Channel<RouterMessage> channel))
                {
                    //Write response
                    channel.Writer.WriteAsync(msg).GetAwaiter().GetResult();

                    //If that's it, clean up and close the channel
                    if (lastPacket.CheckFlag(1))
                    {
                        //Remove
                        waitingResponseSessions.TryRemove(lastPacket.response_token, out channel);

                        //Close channel
                        channel.Writer.Complete();
                    }
                }
                else
                {
                    //Not found! Huh.
                    Log("HandleIncomingMessage", $"Got packet response token {lastPacket.response_token}, but there were no waiting sessions for it! Dropping packet...(although something bad has probably happened)", DeltaLogLevel.High);
                }
            }
            else if(lastPacket.CheckFlag(1))
            {
                //This is an incoming system message
                if(lastPacket.opcode == -1)
                {
                    //Ping message
                    msg.Respond(new byte[]
                    {
                        DeltaConnection.LIB_VERSION_MAJOR,
                        DeltaConnection.LIB_VERSION_MINOR,
                        appVersion.major,
                        appVersion.major
                    }, true);
                }
            } else
            {
                //This is an incoming message, handle as usual
                RouterReceiveMessage(msg);
            }
        }

        private int GetUniqueMessageId()
        {
            int messageId = 0;
            while (messageId == 0)
            {
                messageId = outgoingMessageId;
                outgoingMessageId++;
            }
            return messageId;
        }

        public static short GetChunksInPayload(int payloadSize)
        {
            if (payloadSize > short.MaxValue * MESSAGE_PAYLOAD_SIZE)
                throw new Exception("Payload is WAY too large!");
            return (short)((payloadSize / MESSAGE_PAYLOAD_SIZE) + 1);
        }

        //Called when we get a packet on the transport
        public void _OnReceivePacket(RouterPacket p)
        {
            //Get the global ID of this packet and see if we need to create it
            ulong globalId = p.GetGlobalMessageID();
            RouterMessage msg;
            if (!receivingPackets.TryGetValue(globalId, out msg))
            {
                msg = new RouterMessage(this, p);
                receivingPackets.Add(globalId, msg);
            }

            //Write chunk
            if (msg.WriteChunk(p))
            {
                //We've read all data needed. Handle this packet
                try
                {
                    HandleIncomingMessage(msg, p);
                }
                catch (Exception ex)
                {
                    Log("HandlerWorker", $"Failed to handle incoming message (opcode {msg.opcode} from {msg.sender_addr_router}:{msg.sender_addr_local}) in client code: {ex.Message}{ex.StackTrace}", DeltaLogLevel.Medium);
                }

                //Clean up
                receivingPackets.Remove(globalId);
            }
        }

        public abstract void QueueOutgoingMessage(RouterPacket packet);
        public abstract void RouterReceiveMessage(RouterMessage msg);

        public void Log(string topic, string msg, DeltaLogLevel level)
        {
            baseLogger.Log("BaseRouterIO-" + topic, msg, level);
        }

        public async Task<T> RequestGetObject<T>(short opcode)
        {
            var c = SendMessageGetResponseChannel(opcode, new byte[0]);
            var m = await c.ReadAsync();
            return m.DeserializeAs<T>();
        }
    }
}
