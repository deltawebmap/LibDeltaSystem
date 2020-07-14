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
using LibDeltaSystem.CoreHub;

namespace LibDeltaSystem.Tools
{
    /// <summary>
    /// Managed RPC message builder tool
    /// </summary>
    public static class RPCMessageTool
    {
        private static void SendRPCMsgToUserID(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId user_id, ObjectId? target_server = null)
        {
            BaseClientCoreNetwork net = (BaseClientCoreNetwork)conn.network;
            net.SendRPCEventToUserId(opcode, payload, user_id, target_server);
        }

        private static void SendRPCMsgToServer(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId server_id)
        {
            BaseClientCoreNetwork net = (BaseClientCoreNetwork)conn.network;
            net.SendRPCEventToServerId(opcode, payload, server_id);
        }

        private static void SendRPCMsgToServerTribe(DeltaConnection conn, RPCOpcode opcode, RPCPayload payload, ObjectId server_id, int tribe_id)
        {
            BaseClientCoreNetwork net = (BaseClientCoreNetwork)conn.network;
            net.SendRPCEventToServerTribeId(opcode, payload, server_id, tribe_id);
        }

        /// <summary>
        /// Triggers user groups to be refreshsed
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task SystemNotifyUserGroupReset(DeltaConnection conn, DbUser user)
        {
            BaseClientCoreNetwork net = (BaseClientCoreNetwork)conn.network;
            net.RefreshUserIdGroups(user._id);
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

            //Send
            SendRPCMsgToServerTribe(conn, RPC.RPCOpcode.RPCServer20001ContentSync, payload, server_id, tribe_id);
        }

        public static async Task SendPingToUser(DeltaConnection conn, ObjectId user_id, int nonce)
        {
            //Create payload
            RPCPayload10001Ping payload = new RPCPayload10001Ping
            {
                time = DateTime.UtcNow,
                nonce = nonce
            };

            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCSystem10001Ping, payload, user_id);
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

            //Send
            SendRPCMsgToServerTribe(conn, RPC.RPCOpcode.RPCServer20002PartialUpdate, payload, server_id, tribe_id);
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

            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCSystem10002GuildUpdate, payload, guild._id);
        }
        
        public static async Task SendGuildSetSecureMode(DeltaConnection conn, DbServer guild, bool secure)
        {
            //Create payload
            RPCPayload20004SecureModeToggled payload = new RPCPayload20004SecureModeToggled
            {
                secure = secure
            };

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCServer20004SecureModeToggled, payload, guild._id);
        }

        public static async Task SendGuildPermissionChanged(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload20005GuildPermissionsChanged payload = new RPCPayload20005GuildPermissionsChanged
            {
                flags = guild.permission_flags
            };

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCServer20005GuildPermissionsChanged, payload, guild._id);
        }

        public static async Task SendGuildAdminListUpdated(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload20006GuildAdminListUpdated payload = new RPCPayload20006GuildAdminListUpdated
            {
                admins = guild.admins
            };

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCServer20006GuildAdminListUpdated, payload, guild._id);
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

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCServer20007UserRemovedGuild, payload, guild._id);
        }

        public static async Task SendGuildServerRemoved(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload30004UserServerRemoved payload = new RPCPayload30004UserServerRemoved
            {
                guild_id = guild._id
            };

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCPayload30004UserServerRemoved, payload, guild._id);
        }
        
        public static async Task SendGuildPublicDetailsChanged(DeltaConnection conn, DbServer guild)
        {
            //Create payload
            RPCPayload20010GuildPublicDetailsChanged payload = new RPCPayload20010GuildPublicDetailsChanged
            {
                guild = await NetGuild.GetGuild(conn, guild)
            };

            
            //Send
            SendRPCMsgToServer(conn, RPC.RPCOpcode.RPCServer20010GuildPublicDetailsChanged, payload, guild._id);
        }

        public static async Task SendUserServerClaimed(DeltaConnection conn, DbUser claimer, DbServer guild)
        {
            //Create payload
            RPCPayload30001UserServerClaimed payload = new RPCPayload30001UserServerClaimed
            {
                guild = await NetGuildUser.GetNetGuild(conn, guild, claimer)
            };

            
            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCPayload30001UserServerClaimed, payload, claimer._id);
        }

        public static async Task SendUserServerJoined(DeltaConnection conn, DbUser claimer, DbServer guild)
        {
            //Create payload
            RPCPayload30002UserServerJoined payload = new RPCPayload30002UserServerJoined
            {
                guild = await NetGuildUser.GetNetGuild(conn, guild, claimer),
                cluster = NetCluster.GetCluster(await guild.GetClusterAsync(conn))
            };

            
            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCPayload30002UserServerJoined, payload, claimer._id);
        }

        public static async Task SendUserServerPermissionsChanged(DeltaConnection conn, ObjectId user, DbServer guild)
        {
            //Create payload
            RPCPayload30003UserServerPermissionsChanged payload = new RPCPayload30003UserServerPermissionsChanged
            {
                guild_id = guild._id,
                is_admin = guild.admins.Contains(user)
            };

            
            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCPayload30003UserServerPermissionsChanged, payload, user, guild._id);
        }

        public static async Task SendUserServerRemoved(DeltaConnection conn, ObjectId user, DbServer guild)
        {
            //Create payload
            RPCPayload30004UserServerRemoved payload = new RPCPayload30004UserServerRemoved
            {
                guild_id = guild._id
            };

            
            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCPayload30004UserServerRemoved, payload, user, guild._id);
        }

        public static async Task SendUserArkRpcAck(DeltaConnection conn, ObjectId user, ObjectId guild_id, ObjectId rpc_id, Dictionary<string, string> custom_data)
        {
            //Create payload
            RPCPayload20008ArkRpcAck payload = new RPCPayload20008ArkRpcAck
            {
                rpc_id = rpc_id,
                custom_data = custom_data
            };

            
            //Send
            SendRPCMsgToUserID(conn, RPC.RPCOpcode.RPCServer20008ArkRpcAck, payload, user, guild_id);
        }
    }
}
