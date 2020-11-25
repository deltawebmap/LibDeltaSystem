using LibDeltaSystem.CoreNet;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.RPC.Payloads;
using LibDeltaSystem.Tools;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem
{
    public class DeltaEventMaster
    {
        private DeltaConnection conn;
        
        public DeltaEventMaster(DeltaConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// Notifies the RPC gateway that the groups for a user have changed
        /// </summary>
        /// <param name="userId"></param>
        public void NotifyUserGroupsUpdated(ObjectId userId)
        {
            RPCMessageTool.SystemNotifyUserGroupReset(conn, userId);
        }

        /// <summary>
        /// Sends an ARK RPC callback to the user. This has a very specific use case of only being used when the ARK server responds to a client-issued command
        /// </summary>
        /// <param name="user"></param>
        /// <param name="guild_id"></param>
        /// <param name="rpc_id"></param>
        /// <param name="custom_data"></param>
        public void OnUserArkRpcCallback(ObjectId user, ObjectId guild_id, ObjectId rpc_id, Dictionary<string, string> custom_data)
        {
            RPCPayloadArkRpcCallback payload = new RPCPayloadArkRpcCallback(rpc_id, custom_data);
            RPCMessageTool.SendRPCMsgToUserID(conn, RPC.RPCOpcode.ARK_RPC_CALLBACK, payload, user, guild_id);
        }

        /// <summary>
        /// Notifies a player that their access to a server has changed. Tells a user if they have access to a server, if they're admin, and their tribe ID
        /// </summary>
        public async Task OnUserServerAccessChangedAsync(DbServer server, DbUser user)
        {
            //Fetch the player profile
            var playerProfile = await server.GetUserPlayerProfile(conn, user);

            //Check if they are admin
            bool isAdmin = server.CheckIsUserAdmin(user);

            //Get payload and send
            RPCPayloadServerAccessChanged payload = new RPCPayloadServerAccessChanged(isAdmin, playerProfile);
            RPCMessageTool.SendRPCMsgToUserID(conn, RPC.RPCOpcode.SERVER_ACCESS_CHANGED, payload, user._id, server._id);
        }

        /// <summary>
        /// Notifies a player that a server can now be accessed. This might also be called if they're given admin access to the server. 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        public async Task OnUserServerJoined(DbServer server, DbUser user)
        {
            //Get the net version of the guild
            var net = await NetGuildUser.GetNetGuild(conn, server, user);

            //Get payload and send
            RPCPayloadServerJoined payload = new RPCPayloadServerJoined(net);
            RPCMessageTool.SendRPCMsgToUserID(conn, RPC.RPCOpcode.SERVER_JOINED, payload, user._id);
        }

        /// <summary>
        /// Notifies all members of a server that it has been updated
        /// </summary>
        /// <param name="server"></param>
        public void OnServerUpdated(DbServer server)
        {
            RPCPayloadServerUpdated payload = new RPCPayloadServerUpdated(NetGuild.GetGuild(server));
            RPCMessageTool.SendRPCMsgToServer(conn, RPC.RPCOpcode.SERVER_UPDATED, payload, server._id);
        }

        /// <summary>
        /// Notifies all server members that a server was deleted entirely.
        /// </summary>
        /// <param name="server"></param>
        public void OnServerDeleted(DbServer server)
        {
            RPCPayloadServerDeleted payload = new RPCPayloadServerDeleted(server._id);
            RPCMessageTool.SendRPCMsgToServer(conn, RPC.RPCOpcode.SERVER_DELETED, payload, server._id);
        }
    }
}
