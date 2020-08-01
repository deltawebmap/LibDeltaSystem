using LibDeltaSystem.CoreHub.CoreNetwork;
using LibDeltaSystem.CoreHub.Entities;
using LibDeltaSystem.CoreHub.Extras.OperationProgressStatus;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.RPC;
using LibDeltaSystem.Tools;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreHub
{
    /// <summary>
    /// A CoreNetworkFramework used by many clients. Allows sending RPC events and other actions
    /// </summary>
    public abstract class BaseClientCoreNetwork : CoreNetworkEventClient
    {
        public BaseClientCoreNetwork()
        {
            SubscribeMessageOpcode(CoreNetworkOpcode.REQUEST_HEALTH_REPORT, OnHealthStatusCheck);
            SubscribeMessageOpcode(CoreNetworkOpcode.OPERATION_PROGRESS_UPDATED, OnOperationProgressUpdated);
        }

        public List<OperationProgressServer> operationProgressServers = new List<OperationProgressServer>();
        
        //filterType types:
        //0: User ID (no target server)
        //1: User ID (has target server)
        //2: Server ID (has target server)
        //3: Server ID with Tribe ID (has target server)
        
        /// <summary>
        /// Sends an RPC event
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="filterData"></param>
        /// <param name="actionType"></param>
        /// <param name="actionData"></param>
        private void _SendRPCEvent(RPCOpcode opcode, byte filterType, byte[] filterData, byte actionType, byte[] actionData)
        {
            //Create buffer to use
            byte[] payload;
            using(MemoryStream output = new MemoryStream())
            {
                //Write data
                output.WriteByte(filterType);
                output.WriteByte((byte)filterData.Length);
                output.Write(filterData, 0, filterData.Length);
                output.WriteByte(actionType);

                //Write opcode
                output.Write(BitConverter.GetBytes((int)opcode), 0, 4);

                //Write decompressed length
                output.Write(BitConverter.GetBytes((ushort)actionData.Length), 0, 2);

                //Make room for compressed length, even if it won't be used
                long compressedLenPos = output.Position;
                output.Write(BitConverter.GetBytes((ushort)actionData.Length), 0, 2);

                //If the length is <=255 bytes in length, don't compress it
                if (actionData.Length <= 255)
                {
                    //Write compressed
                    output.Write(actionData, 0, actionData.Length);
                } else
                {
                    //GZIP the action data
                    using (GZipStream gz = new GZipStream(output, CompressionMode.Compress, true))
                    {
                        gz.Write(actionData, 0, actionData.Length);
                    }

                    //Rewind and write compressed length
                    ushort compressedLen = (ushort)(compressedLenPos + 2 - output.Position);
                    output.Position = compressedLenPos;
                    output.Write(BitConverter.GetBytes(compressedLen), 0, 2);
                }

                //Send
                output.Position = 0;
                payload = output.ToArray();
            }

            //Send this to all RPC servers
            var servers = list.FindAllServersOfType(CoreNetworkServerType.RPC_SERVER);
            foreach (var s in servers)
                SendMessage(s, CoreNetworkOpcode.RPC_EVENT, payload);
            if (servers.Count == 0)
                delta.Log("BaseClientCoreNetwork-RPC", "There are no registered RPC servers! RPC events will never be received.", DeltaLogLevel.High);
        }

        private void _SendRPCEvent(RPCOpcode opcode, byte filterType, byte[] filterData, byte actionType, RPCPayload actionData)
        {
            _SendRPCEvent(opcode, filterType, filterData, actionType, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(actionData)));
        }

        public void SendRPCEventToUserId(RPCOpcode opcode, RPCPayload payload, ObjectId user_id, ObjectId? target_server = null)
        {
            //Create filter
            byte filterType;
            byte[] filterPayload;
            if(!target_server.HasValue)
            {
                filterPayload = new byte[12];
                BinaryTool.WriteMongoID(filterPayload, 0, user_id);
                filterType = 0;
            } else
            {
                filterPayload = new byte[24];
                BinaryTool.WriteMongoID(filterPayload, 0, user_id);
                BinaryTool.WriteMongoID(filterPayload, 12, target_server.Value);
                filterType = 1;
            }

            //Send
            _SendRPCEvent(opcode, filterType, filterPayload, 0, payload);
        }

        public void SendRPCEventToServerId(RPCOpcode opcode, RPCPayload payload, ObjectId server_id)
        {
            //Create filter
            byte[] filterPayload = new byte[12];
            BinaryTool.WriteMongoID(filterPayload, 0, server_id);

            //Send
            _SendRPCEvent(opcode, 2, filterPayload, 0, payload);
        }

        public void SendRPCEventToServerTribeId(RPCOpcode opcode, RPCPayload payload, ObjectId server_id, int tribe_id)
        {
            //Create filter
            byte[] filterPayload = new byte[16];
            BinaryTool.WriteMongoID(filterPayload, 0, server_id);
            BinaryTool.WriteInt32(filterPayload, 12, tribe_id);

            //Send
            _SendRPCEvent(opcode, 3, filterPayload, 0, payload);
        }

        public void SendRPCEventToServerTribeId(RPCOpcode opcode, RPCPayload payload, DbServer server, int tribe_id)
        {
            SendRPCEventToServerTribeId(opcode, payload, server._id, tribe_id);
        }

        public void RefreshUserIdGroups(ObjectId user_id)
        {
            //Create filter
            byte[] filterPayload = new byte[13];
            filterPayload[0] = 0;
            BinaryTool.WriteMongoID(filterPayload, 1, user_id);

            //Send this to all RPC servers
            var servers = list.FindAllServersOfType(CoreNetworkServerType.RPC_SERVER);
            foreach (var s in servers)
                SendMessage(s, CoreNetworkOpcode.RPC_REFRESH_GROUPS, filterPayload);
        }

        public async Task<Dictionary<string, object>> RequestServerHealth(CoreNetworkServer server)
        {
            //Request
            var health = await SendMessageGetResponse(server, CoreNetworkOpcode.REQUEST_HEALTH_REPORT, new byte[0]);

            //Parse
            Dictionary<string, object> output = new Dictionary<string, object>();
            int offset = 1;
            for (int i = 0; i < health[0]; i++)
            {
                string key = Encoding.UTF8.GetString(health, offset + 1, health[offset]);
                offset += key.Length + 1;
                byte type = health[offset];
                byte len = health[offset + 1];
                offset += 2;
                string value;
                if (type == 1)
                    value = LibDeltaSystem.Tools.BinaryTool.ReadInt32(health, offset).ToString();
                else
                    value = Encoding.UTF8.GetString(health, offset, len);
                offset += len;
                output.Add(key, value);
            }

            return output;
        }

        public async Task<CoreStatusResponse> RequestServerStats(CoreNetworkServer server)
        {
            byte[] payload = await SendMessageGetResponse(server, CoreNetworkOpcode.MESSAGE_STATS, new byte[0]);
            return new CoreStatusResponse(payload);
        }

        /// <summary>
        /// Deploys a new server on the specified host and returns it's ID
        /// </summary>
        /// <param name="host"></param>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public async Task<ushort> DeployNewServer(CoreNetworkServer host, CoreNetworkServerType type, string config, byte count)
        {
            //Build payload
            byte[] payload = new byte[4 + Encoding.UTF8.GetByteCount(config)];
            BinaryTool.WriteInt16(payload, 0, (short)type);
            payload[2] = count;
            payload[3] = 0x00; //reserved for later usage
            Encoding.UTF8.GetBytes(config, 0, config.Length, payload, 4);

            //Send command
            var response = await SendMessageGetResponse(host, CoreNetworkOpcode.PROCESSMAN_DEPLOY, payload);
            
            //Check status
            if(response[0] == 0x00)
            {
                //OK
                return BinaryTool.ReadUInt16(response, 1);
            } else
            {
                //Failed
                throw new Exception(Encoding.UTF8.GetString(response, 1, response.Length - 1));
            }
        }

        public void RemoteLog(string topic, string message, DeltaLogLevel level)
        {
            //Check if this is blacklisted topic. These topics generally just cause an infinite loop
            if ((topic.StartsWith("CoreHub") && level == DeltaLogLevel.Debug) || topic == "CoreHub-_BeginListening")
                return;

            //Serialize
            byte[] ser = new byte[2 + Encoding.UTF8.GetByteCount(topic) + Encoding.UTF8.GetByteCount(message)];
            ser[0] = (byte)level;
            ser[1] = (byte)Encoding.UTF8.GetByteCount(topic);
            Encoding.UTF8.GetBytes(topic, 0, topic.Length, ser, 2);
            Encoding.UTF8.GetBytes(message, 0, message.Length, ser, 2 + ser[1]);

            //Write
            var servers = list.FindAllServersOfType(CoreNetworkServerType.HUB_SERVER);
            foreach (var s in servers)
                SendMessage(s, CoreNetworkOpcode.REMOTE_LOG, ser);
        }

        private byte[] OnHealthStatusCheck(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] payload)
        {
            HealthStatusWriter writer = new HealthStatusWriter();
            HealthStatusRequested(writer);
            return writer.ToBytes();
        }

        private byte[] OnOperationProgressUpdated(CoreNetworkServer server, CoreNetworkOpcode opcode, byte[] payload)
        {
            //Extract the data
            ushort token = BitConverter.ToUInt16(payload, 0);

            //Find a server with a matching token
            OperationProgressServer recipient = null;
            lock(operationProgressServers)
            {
                foreach (var s in operationProgressServers)
                {
                    if (s.token == token)
                        recipient = s;
                }
            }

            //Send
            if(recipient != null)
                recipient.OnOperationResponse(payload);

            return new byte[0];
        }

        /// <summary>
        /// Requests clients to write health
        /// </summary>
        /// <param name="writer"></param>
        public virtual void HealthStatusRequested(HealthStatusWriter writer)
        {
            writer.WriteInt("CORENET.STAT.OUTGOING_QUEUE_SIZE", stat_outgoingQueueSize);
            writer.WriteInt("CORENET.STAT.PREVIOUS_ID_COUNT", stat_previousIdCount);
            writer.WriteInt("CORENET.STAT.MESSAGES_SENT", stat_messagesSent);
            writer.WriteInt("CORENET.STAT.MESSAGES_RECEIVED", stat_messagesReceived);
            writer.WriteInt("CORENET.STAT.MESSAGES_RECEIVED_DUPLICATE", stat_messagesReceivedDuplicate);
            writer.WriteInt("CORENET.STAT.MESSAGES_RESENT", stat_messagesResent);
            writer.WriteInt("CORENET.STAT.MESSAGES_ACKED", stat_messagesAcked);
            writer.WriteInt("CORENET.STAT.MESSAGES_AUTH_FAILED", stat_messagesAuthFailed);
            writer.WriteInt("CORENET.ME.SERVER_ID", me.id);
            writer.WriteString("CORENET.ME.SERVER_TYPE", me.type.ToString());
            writer.WriteInt("DELTA.UPTIME_SECONDS", (int)(DateTime.UtcNow - delta.start_time).TotalSeconds);
            if(delta.web_server != null)
            {
                writer.WriteInt("WEB.LISTEN_PORT", delta.web_server.port);
                writer.WriteString("WEB.REQUESTS_HANDLED", delta.web_server.stat_requests_handled.ToString());
            }
        }
    }
}
