using LibDeltaSystem.Db.Content;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    /// <summary>
    /// Checks to make sure that the server requested can be accessed by this user and gets their tribe
    /// By default, even people with admin access aren't allowed
    /// </summary>
    public abstract class CheckedTribeServerDeltaService : ArkServerDeltaService
    {
        /// <summary>
        /// The authorized tribe ID
        /// </summary>
        public int? tribeId;

        /// <summary>
        /// Authorized tribe data
        /// </summary>
        public DbTribe tribe;
        
        public CheckedTribeServerDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Run base
            if (!await base.SetArgs(args))
                return false;

            //Get user tribe ID
            int? myTribeId = await server.TryGetTribeIdAsync(conn, user.steam_id);

            //Check
            bool allowed = CheckTribeID(myTribeId);

            //If the check failed, abort
            if(!allowed)
            {
                await WriteString("Tribe Not Authenticated", "text/plain", 403);
                return false;
            }

            //Get tribe info
            tribeId = myTribeId;
            if (myTribeId.HasValue)
            {
                tribe = await conn.GetTribeByTribeIdAsync(server.id, myTribeId.Value);

                //Make sure we got data
                if (tribe == null)
                {
                    await WriteString("Could not get tribe info for ID " + tribeId, "text/plain", 404);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks to see if a user is allowed to use this server. Return null to cancel, or else return a valid tribe ID
        /// </summary>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public virtual bool CheckTribeID(int? tribeId)
        {
            return tribeId.HasValue;
        }
    }
}
