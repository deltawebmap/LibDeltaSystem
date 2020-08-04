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

            return true;
        }

        public FilterDefinition<T> GetServerTribeFilter<T>()
        {
            var builder = Builders<T>.Filter;
            if(canRequestOtherTribes)
            {
                //This user is admin, they may request whatever they want
                return builder.Eq("server_id", server._id);
            } else if(!canRequestOtherTribes && profile != null)
            {
                //This player can only request their own tribe
                return builder.Eq("server_id", server._id) & builder.Eq("tribe_id", profile.tribe_id);
            } else
            {
                //This player doesn't have a tribe and can't request others!
                return builder.Eq("server_id", server._id) & builder.Eq<int>("tribe_id", 0); //Shouldn't return other tribes
            }
        }
    }
}
