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

namespace LibDeltaSystem.Tools
{
    /// <summary>
    /// Managed RPC message builder tool
    /// </summary>
    public static class RPCMessageTool
    {
        /// <summary>
        /// Triggers user groups to be refreshsed
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task SystemNotifyUserGroupReset(DeltaConnection conn, DbUser user)
        {
            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendNotifyUserGroupsUpdated(user._id);
        }
        
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
        
        public static async Task SendGuildSetSecureMode(DeltaConnection conn, DbServer guild, bool secure)
        {
            //Create payload
            RPCPayload20004SecureModeToggled payload = new RPCPayload20004SecureModeToggled
            {
                secure = secure
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServer(RPC.RPCOpcode.RPCServer20004SecureModeToggled, payload, guild._id);
        }

        public static async Task SendGuildPermissionChanged(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload20005GuildPermissionsChanged payload = new RPCPayload20005GuildPermissionsChanged
            {
                flags = guild.permission_flags
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServer(RPC.RPCOpcode.RPCServer20005GuildPermissionsChanged, payload, guild._id);
        }

        public static async Task SendGuildAdminListUpdated(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload20006GuildAdminListUpdated payload = new RPCPayload20006GuildAdminListUpdated
            {
                admins = guild.admins
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServer(RPC.RPCOpcode.RPCServer20006GuildAdminListUpdated, payload, guild._id);
        }
        
        public static async Task SendGuildUserRemoved(DeltaConnection conn, DbServer guild, DbUser user)
        {
            //Create payload
            RPCPayload20007UserRemovedGuild payload = new RPCPayload20007UserRemovedGuild
            {
                icon = user.profile_image_url,
                name = user.screen_name,
                steam_id = user.steam_id,
                user_id = user._id
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToServer(RPC.RPCOpcode.RPCServer20007UserRemovedGuild, payload, guild._id);
        }

        public static async Task SendUserServerClaimed(DeltaConnection conn, DbUser claimer, DbServer guild)
        {
            //Create payload
            RPCPayload30001UserServerClaimed payload = new RPCPayload30001UserServerClaimed
            {
                guild = await NetGuildUser.GetNetGuild(conn, guild, claimer)
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToUserID(RPC.RPCOpcode.RPCPayload30001UserServerClaimed, payload, claimer);
        }

        public static async Task SendUserServerJoined(DeltaConnection conn, DbUser claimer, DbServer guild)
        {
            //Create payload
            RPCPayload30002UserServerJoined payload = new RPCPayload30002UserServerJoined
            {
                guild = await NetGuildUser.GetNetGuild(conn, guild, claimer),
                cluster = NetCluster.GetCluster(await guild.GetClusterAsync(conn))
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToUserID(RPC.RPCOpcode.RPCPayload30002UserServerJoined, payload, claimer);
        }

        public static async Task SendUserServerPermissionsChanged(DeltaConnection conn, ObjectId user, DbServer guild)
        {
            //Create payload
            RPCPayload30003UserServerPermissionsChanged payload = new RPCPayload30003UserServerPermissionsChanged
            {
                guild_id = guild._id,
                is_admin = guild.admins.Contains(user)
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToUserID(RPC.RPCOpcode.RPCPayload30003UserServerPermissionsChanged, payload, user, guild._id);
        }

        public static async Task SendUserServerRemoved(DeltaConnection conn, ObjectId user, DbServer guild)
        {
            //Create payload
            RPCPayload30004UserServerRemoved payload = new RPCPayload30004UserServerRemoved
            {
                guild_id = guild._id
            };

            //Get RPC
            var rpc = conn.GetRPC();

            //Send
            await rpc.SendRPCMsgToUserID(RPC.RPCOpcode.RPCPayload30004UserServerRemoved, payload, user, guild._id);
        }
    }
}
