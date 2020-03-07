using LibDeltaSystem.Db.Content;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    /// <summary>
    /// Authorizes a tribe requested from a URL
    /// </summary>
    public abstract class RequestTribeServerDeltaService : ArkServerDeltaService
    {
        /// <summary>
        /// The authorized tribe ID
        /// </summary>
        public int? tribeId;

        /// <summary>
        /// Authorized tribe data. Could be null
        /// </summary>
        public DbTribe tribe;

        public RequestTribeServerDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Run base
            if (!await base.SetArgs(args))
                return false;

            //Get the tribe ID from the url
            int? myTribeId;
            if(args[DeltaWebServiceDefinition.ARG_TRIBE] == "*")
            {
                myTribeId = null;
            } else
            {
                //Try to parse int
                if (!int.TryParse(args[DeltaWebServiceDefinition.ARG_TRIBE], out int trR))
                {
                    await WriteString("Tribe ID Not Valid", "text/plain", 403);
                    return false;
                }

                //Set
                myTribeId = trR;
            }

            //Check
            EndDebugCheckpoint("Check Tribe Authentication");
            if (!await CheckIfTribeIdAllowed(myTribeId))
            {
                await WriteString("Tribe Not Authenticated", "text/plain", 403);
                return false;
            }

            //Set data
            tribeId = myTribeId;
            if(myTribeId.HasValue)
            {
                //Get tribe info
                EndDebugCheckpoint("Get tribe info");
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
        /// Checks if a tribe ID is allowed to be used
        /// </summary>
        /// <param name="tribeId"></param>
        /// <returns></returns>
        public abstract Task<bool> CheckIfTribeIdAllowed(int? tribeId);
    }
}
