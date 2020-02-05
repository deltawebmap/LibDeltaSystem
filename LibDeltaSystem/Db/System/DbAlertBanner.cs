using LibDeltaSystem.RPC.Payloads;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Alert banners are dismissible notifications that appear in-app. They aren't significant enough to be sent as a push notification.
    /// They can also target either a user or a server.
    /// Banners sent with the same targets and opcode will replace the old one, but will have a new ID
    /// </summary>
    public class DbAlertBanner : DbBaseSystem
    {
        /// <summary>
        /// This is the target user ID for this
        /// </summary>
        public ObjectId? target_user_id { get; set; }

        /// <summary>
        /// This is the target server ID for this. If null, this will appear as a global banner
        /// </summary>
        public ObjectId? target_server_id { get; set; }

        /// <summary>
        /// The target opcode of this message
        /// https://docs.google.com/spreadsheets/d/1OZ7svV3-e8hszxJwW2D5-FMonLX4AKFM-0ucnE7Xhak/edit?usp=sharing
        /// </summary>
        public int opcode { get; set; }

        /// <summary>
        /// The time this was pushed
        /// </summary>
        public DateTime time { get; set; }

        /// <summary>
        /// Extra data provided to the application
        /// </summary>
        public Dictionary<string, string> extras { get; set; }

        public static async Task PushBanner(DeltaConnection conn, int opcode, ObjectId? targetServer, ObjectId? targetUser, Dictionary<string, string> extras)
        {
            //Check to see if everything checks out
            if (targetServer == null && targetUser == null)
                throw new Exception("Must send banner to a target!");

            //Create a banner object
            DbAlertBanner banner = new DbAlertBanner
            {
                time = DateTime.UtcNow,
                extras = extras,
                opcode = opcode,
                target_server_id = targetServer,
                target_user_id = targetUser,
                _id = ObjectId.GenerateNewId()
            };

            //Create RPC payload for this
            RPCPayloadAlertBannerPush payload = new RPCPayloadAlertBannerPush
            {
                banner = banner
            };

            //Send out an RPC message notifiying clients of this
            if(targetUser != null)
                conn.GetRPC().SendRPCMessageToUser(RPC.RPCOpcode.AlertBannerPush, payload, targetUser.Value, RPCType.RPC);
            else
                conn.GetRPC().SendRPCMessageToServer(RPC.RPCOpcode.AlertBannerPush, payload, targetServer.Value, RPCType.RPC);

            //Push to DB
            await conn.system_alert_banners.InsertOneAsync(banner);
        }
    }
}
