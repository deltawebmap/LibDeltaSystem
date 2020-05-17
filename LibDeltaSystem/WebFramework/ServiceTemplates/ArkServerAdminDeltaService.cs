using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    public abstract class ArkServerAdminDeltaService : ArkServerDeltaService
    {
        public ArkServerAdminDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Make sure we're an admin
            if(server.CheckIsUserAdmin(user))
            {
                await OnAuthenticatedRequest();
            } else
            {
                await WriteString("Only server admins can access this endpoint.", "text/plain", 401);
            }
        }

        public abstract Task OnAuthenticatedRequest();
    }
}
