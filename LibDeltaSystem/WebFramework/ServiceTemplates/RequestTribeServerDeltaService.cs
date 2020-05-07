using LibDeltaSystem.Db.Content;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
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
        /// The requested tribe ID in the URL. Null if a wildcard was specified
        /// </summary>
        public int? requestedTribeId;

        /// <summary>
        /// The active user's player profile
        /// </summary>
        public DbPlayerProfile profile;

        /// <summary>
        /// Is this user admin?
        /// </summary>
        public bool admin;

        public RequestTribeServerDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Run base
            if (!await base.SetArgs(args))
                return false;

            //Get the player profile
            profile = await server.GetUserPlayerProfile(conn, user);
            admin = server.IsUserAdmin(user);

            //If no profile was found, this user doesn't even have access to this server
            if (profile == null && !admin)
            {
                await WriteString("You do not have access to this server.", "text/plain", 403);
                return false;
            }

            //Get requested tribe ID
            if (args[DeltaWebServiceDefinition.ARG_TRIBE] == "*")
            {
                requestedTribeId = null;
            }
            else
            {
                if (!int.TryParse(args[DeltaWebServiceDefinition.ARG_TRIBE], out int trR))
                {
                    await WriteString("Tribe ID Not Valid", "text/plain", 403);
                    return false;
                }
                requestedTribeId = trR;
            }

            //Make sure that non-admins are only attempting to fetch their own data (or a wildcard)
            if(!admin)
            {
                if (requestedTribeId.HasValue && requestedTribeId.GetValueOrDefault() != profile.tribe_id)
                {
                    await WriteString("You do not have access to this tribe ID", "text/plain", 403);
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
        public bool CheckIfTribeIdAllowed(int tribeId)
        {
            if(admin)
            {
                if (requestedTribeId.HasValue)
                    return requestedTribeId.Value == tribeId;
                else
                    return true;
            } else
            {
                return tribeId == profile.tribe_id;
            }
        }

        public FilterDefinition<T> GetServerTribeFilter<T>()
        {
            var builder = Builders<T>.Filter;
            if(admin)
            {
                //This user is admin, they may request whatever they want
                if (requestedTribeId.HasValue)
                    return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", requestedTribeId.Value);
                else
                    return builder.Eq("server_id", server._id);
            } else
            {
                //This player may only fetch their own tribe. We checked that this is the selected tribe when we first requested this.
                return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", profile.tribe_id);
            }
        }
    }
}
