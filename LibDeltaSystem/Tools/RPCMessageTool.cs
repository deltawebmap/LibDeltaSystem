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

namespace LibDeltaSystem.Tools
{
    /// <summary>
    /// Managed RPC message builder tool
    /// </summary>
    public static class RPCMessageTool
    {
        /// <summary>
        /// Sent when there is a content update (when the database changes)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="server_id"></param>
        /// <param name="tribe_id"></param>
        /// <returns></returns>
        public static async Task SendDbContentUpdateMessage(DeltaConnection conn, RPCSyncType type, object data, ObjectId server_id, int tribe_id)
        {
            //Create payload
            RPCPayload20001ContentSync payload = new RPCPayload20001ContentSync
            {
                content = data,
                time = DateTime.UtcNow,
                tribe_id = tribe_id,
                type = type
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServerTribe(RPC.RPCOpcode.RPCServer20001ContentSync, payload, server_id, tribe_id);
        }

        public static async Task SendPingToUser(DeltaConnection conn, ObjectId user_id, int nonce)
        {
            //Create payload
            RPCPayload10001Ping payload = new RPCPayload10001Ping
            {
                time = DateTime.UtcNow,
                nonce = nonce
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToUserID(RPC.RPCOpcode.RPCSystem10001Ping, payload, user_id);
        }

        public static async Task SendDbUpdatePartial(DeltaConnection conn, RPCSyncType type, ObjectId server_id, int tribe_id, string object_id, RPCPayload20002PartialUpdate.RPCPayload20002PartialUpdate_Update updates)
        {
            //Create payload
            RPCPayload20002PartialUpdate payload = new RPCPayload20002PartialUpdate
            {
                time = DateTime.UtcNow,
                tribe_id = tribe_id,
                type = type,
                id = object_id,
                updates = updates
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServerTribe(RPC.RPCOpcode.RPCServer20002PartialUpdate, payload, server_id, tribe_id);
        }

        public static async Task SendGuildUpdate(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload10002GuildUpdate payload = new RPCPayload10002GuildUpdate
            {
                guild = await NetGuild.GetGuild(conn, guild),
                server_id = guild.id,
                time = DateTime.UtcNow
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServer(RPC.RPCOpcode.RPCSystem10002GuildUpdate, payload, guild._id);
        }
    }
}
