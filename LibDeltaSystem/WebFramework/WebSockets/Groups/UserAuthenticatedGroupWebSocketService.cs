using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.Groups
{
    /// <summary>
    /// Simply authenticates a user before continuing. Does not do anything additional with groups
    /// </summary>
    public abstract class UserAuthenticatedGroupWebSocketService : GroupWebSocketService
    {
        /// <summary>
        /// The user authenticated
        /// </summary>
        public DbUser user;

        /// <summary>
        /// The token used to issue this request
        /// </summary>
        public DbToken token;

        public UserAuthenticatedGroupWebSocketService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> OnPreRequest()
        {
            //Get token
            string tokenString = GetAuthToken();
            if (tokenString == null)
            {
                await WriteString("Not Authorized", "text/plain", 401);
                return false;
            }

            //Authenticate this token
            EndDebugCheckpoint("Authenticate Token");
            token = await conn.GetTokenByTokenAsync(tokenString);
            if (token == null)
            {
                await WriteString("Not Authorized", "text/plain", 401);
                return false;
            }

            //Get user
            EndDebugCheckpoint("Authenticate User w/ Token");
            user = await conn.GetUserByIdAsync(token.user_id);
            if (user == null)
            {
                await WriteString("Not Authorized", "text/plain", 401);
                return false;
            }

            return true;
        }

        private string GetAuthToken()
        {
            if (!e.Request.Query.ContainsKey("access_token"))
                return null;
            return e.Request.Query["access_token"];
        }
    }
}
