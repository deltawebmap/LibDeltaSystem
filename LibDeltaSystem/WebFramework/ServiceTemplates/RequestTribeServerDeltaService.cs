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

        /// <summary>
        /// Can this user request other tribes? Usually this is false, unless this user as an admin and secure mode is off
        /// </summary>
        public bool canRequestOtherTribes;

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
            canRequestOtherTribes = admin && !server.secure_mode;

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

        public FilterDefinition<T> GetServerTribeFilter<T>()
        {
            var builder = Builders<T>.Filter;
            if(admin && canRequestOtherTribes)
            {
                //This user is admin, they may request whatever they want
                if (requestedTribeId.HasValue)
                    return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", requestedTribeId.Value);
                else
                    return builder.Eq("server_id", server._id);
            } else if(admin && !canRequestOtherTribes)
            {
                //This user is admin, but they can't request whatever they want because secure mode is on
                if (requestedTribeId.GetValueOrDefault(-2) == profile.tribe_id || !requestedTribeId.HasValue)
                    return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", profile.tribe_id);
                else
                    return builder.Eq("server_id", server._id) & builder.Eq<int?>("tribe_id", null);
            } else
            {
                //This player may only fetch their own tribe. We checked that this is the selected tribe when we first requested this.
                return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", profile.tribe_id);
            }
        }
    }
}
