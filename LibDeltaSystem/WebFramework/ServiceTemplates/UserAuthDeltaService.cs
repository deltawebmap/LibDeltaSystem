using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    public abstract class UserAuthDeltaService : DeltaWebService
    {
        /// <summary>
        /// The user authenticated
        /// </summary>
        public DbUser user;

        /// <summary>
        /// The token used to issue this request
        /// </summary>
        public DbToken token;
        
        public UserAuthDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> OnPreRequest()
        {
            //Get token
            string tokenString = GetAuthToken();
            if(tokenString == null)
            {
                await WriteString("Not Authorized", "text/plain", 401);
                return false;
            }

            //Authenticate this token
            token = await conn.GetTokenByTokenAsync(tokenString);
            if (token == null)
            {
                await WriteString("Not Authorized", "text/plain", 401);
                return false;
            }

            //Get user
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
            if (!e.Request.Headers.ContainsKey("authorization"))
                return null;
            string h = e.Request.Headers["authorization"];
            if (!h.StartsWith("Bearer "))
                return null;
            return h.Substring("Bearer ".Length);
        }
    }
}
