using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.OpcodeSock
{
    public abstract class DeltaOpcodeUserWebSocketService : DeltaOpcodeWebSocketService
    {
        public DeltaOpcodeUserWebSocketService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            RegisterCommandHandler("LOGIN", OnLoginRequest);
        }

        public DbUser user;
        public DbToken token;

        private async Task OnLoginRequest(JObject data)
        {
            //Check if already logged in
            if (user != null)
            {
                await SendLoginStatus(false, "Already logged in. Disconnect and reconnect first.");
                return;
            }

            //Validate
            if(!UtilValidateJObject(data, out string validateError, new JObjectValidationParameter("access_token", JTokenType.String)))
            {
                await SendLoginStatus(false, validateError);
                return;
            }

            //Get token
            token = await conn.GetTokenByTokenAsync((string)data["access_token"]);
            if (token == null)
            {
                await SendLoginStatus(false, "Token Invalid");
                return;
            }

            //Get user
            user = await conn.GetUserByIdAsync(token.user_id);
            if (user == null)
            {
                await SendLoginStatus(false, "User Invalid (bad!)");
                return;
            }

            //Issue OK
            await SendLoginStatus(true, "OK; Logged in user " + user.id);

            //Let user space code handle this
            await OnUserLoginSuccess();
        }

        private async Task SendLoginStatus(bool success, string message)
        {
            JObject msg = new JObject();
            msg["success"] = success;
            msg["message"] = message;
            await SendMessage("LOGIN_STATUS", msg);
        }

        public abstract Task OnUserLoginSuccess();
    }
}
