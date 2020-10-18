using LibDeltaSystem.RPC.Payloads.System;
using LibDeltaSystem.RPC.Payloads.Server;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem.RPC.Payloads.Entities;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.RPC.Payloads.User;
using LibDeltaSystem.RPC;
using Newtonsoft.Json;

namespace LibDeltaSystem.Tools
{
    /// <summary>
    /// Managed RPC message builder tool
    /// </summary>
    public static class RPCMessageTool
    {
        public static void SendRPCMsgToUserID(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId user_id, ObjectId? target_server = null)
        {
            //Create filter
            byte filterType;
            byte[] filterPayload;
            if (!target_server.HasValue)
            {
                filterPayload = new byte[12];
                BinaryTool.WriteMongoID(filterPayload, 0, user_id);
                filterType = 0;
            }
            else
            {
                filterPayload = new byte[24];
                BinaryTool.WriteMongoID(filterPayload, 0, user_id);
                BinaryTool.WriteMongoID(filterPayload, 12, target_server.Value);
                filterType = 1;
            }

            //Send
            byte[] actionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            conn.net.SendRPCCommand(opcode, filterType, filterPayload, 0, actionData);
        }

        public static void SendRPCMsgToServer(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId server_id)
        {
            //Create filter
            byte[] filterPayload = new byte[12];
            BinaryTool.WriteMongoID(filterPayload, 0, server_id);

            //Send
            byte[] actionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            conn.net.SendRPCCommand(opcode, 2, filterPayload, 0, actionData);
        }

        public static void SendRPCMsgToServerTribe(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId server_id, int tribe_id)
        {
            //Create filter
            byte[] filterPayload = new byte[16];
            BinaryTool.WriteMongoID(filterPayload, 0, server_id);
            BinaryTool.WriteInt32(filterPayload, 12, tribe_id);

            //Send
            byte[] actionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            conn.net.SendRPCCommand(opcode, 3, filterPayload, 0, actionData);
        }

        public static void SystemNotifyUserGroupReset(DeltaConnection conn, ObjectId user)
        {
            //Create filter
            byte[] filterPayload = new byte[13];
            filterPayload[0] = 0;
            BinaryTool.WriteMongoID(filterPayload, 1, user);

            //Send
            conn.net.SendRPCCommand((RPCOpcode)0, 0, filterPayload, 1, new byte[0]);
        }
    }
}
