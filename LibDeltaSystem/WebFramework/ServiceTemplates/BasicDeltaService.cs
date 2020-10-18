using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    public abstract class BasicDeltaService : DeltaWebService
    {
        public BasicDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }
    }
}
