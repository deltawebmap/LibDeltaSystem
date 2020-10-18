using LibDeltaSystem.CoreNet;
using LibDeltaSystem.Db.System;
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

        }

        /// <summary>
        /// Notifies a player that their access to a server has changed. Tells a user if they have access to a server, if they're admin, and their tribe ID
        /// </summary>
        public async Task OnUserServerAccessChangedAsync(DbServer server, DbUser user)
        {
            //Fetch the player profile
            var playerProfile = await server.GetUserPlayerProfile(conn, user);
        }

        /// <summary>
        /// Notifies a player that a server can now be accessed. This might also be called if they're given admin access to the server. 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="user"></param>
        public void OnUserServerJoined(DbServer server, DbUser user)
        {

        }

        /// <summary>
        /// Notifies all members of a server that it has been updated
        /// </summary>
        /// <param name="server"></param>
        public void OnServerUpdated(DbServer server)
        {

        }

        /// <summary>
        /// Notifies all server members that a server was deleted entirely.
        /// </summary>
        /// <param name="server"></param>
        public void OnServerDeleted(DbServer server)
        {

        }
    }
}
