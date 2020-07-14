using LibDeltaSystem.Db.System;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.ServiceTemplates
{
    /// <summary>
    /// Gets a server from the URL template.
    /// WARNING: THIS IS UNCHECKED, THIS USER MAY NOT HAVE ACCESS TO THIS SERVER
    /// </summary>
    public abstract class ArkServerDeltaService : UserAuthDeltaService
    {
        /// <summary>
        /// The ARK server that was requested via URL
        /// </summary>
        public DbServer server;
        
        public ArkServerDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Try to parse the server ID
            if (!ObjectId.TryParse(args[DeltaWebServiceDefinition.ARG_SERVER], out ObjectId server_id))
            {
                await WriteString("Not a valid server ID", "text/plain", 400);
                return false;
            }

            //Get this server
            server = await conn.GetServerByIdAsync(server_id);
            if(server == null)
            {
                await WriteString("Server not found", "text/plain", 404);
                return false;
            }

            return true;
        }
    }
}
