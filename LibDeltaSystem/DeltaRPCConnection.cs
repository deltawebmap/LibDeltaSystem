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
using LibDeltaSystem.Tools.InternalComms;

namespace LibDeltaSystem
{
    public class DeltaRPCConnection : InternalCommClient
    {
        public DeltaRPCConnection(DeltaConnection conn, byte[] key, IPEndPoint endpoint) : base(conn, key, endpoint)
        {

        }

        /// <summary>
        /// Should never be called
        /// </summary>
        /// <param name="opcode"></param>
        /// <param name="payloads"></param>
        /// <returns></returns>
        public override async Task HandleMessage(int opcode, Dictionary<string, byte[]> payloads)
        {
            
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
            RawSendMessage(1, new Dictionary<string, byte[]>
            {
                {"FILTER", filterMsg },
                {"DATA", message }
            });
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
    }
}
