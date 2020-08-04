using LibDeltaSystem.WebFramework.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework
{
    /// <summary>
    /// An HTTP server that can be used to handle requests to the system
    /// </summary>
    public class DeltaWebServer
    {
        public DeltaWebServer(DeltaConnection conn, int port)
        {
            this.conn = conn;
            this.port = port;
            this.services = new List<DeltaWebServiceDefinition>();
            this.start = DateTime.UtcNow;
            conn.AttachWebServer(this);
        }

        public void Log(string topic, string msg)
        {
            conn.Log(topic, msg, DeltaLogLevel.Debug);
        }

        public Task RunAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, port);

                })
                .UseStartup<DeltaWebServer>()
                .Configure(Configure)
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Run(OnHTTPRequest);
        }

        public List<DeltaWebServiceDefinition> services;
        public DeltaConnection conn;
        public int port;
        public DateTime start;

        public long stat_requests_handled;

        public List<string> exposedHeaders = new List<string>(); //List of headers that are exposed to CORS
        
        public async Task OnHTTPRequest(HttpContext e)
        {
            //Do CORS stuff
            e.Response.Headers.Add("X-Delta-Server-ID", conn.network.me.id.ToString());
            e.Response.Headers.Add("X-Delta-Server-Type", conn.network.me.type.ToString());
            e.Response.Headers.Add("Access-Control-Expose-Headers", GetExposedHeadersString());
            e.Response.Headers.Add("Access-Control-Allow-Headers", GetExposedHeadersString());
            e.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            e.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE, PUT, PATCH");
            if (e.Request.Method.ToUpper() == "OPTIONS")
            {
                await WriteStringToBody(e, "Dropping OPTIONS request. Hello CORS!", "text/plain", 200);
                return;
            }

            //Log
            conn.Log("DeltaWebServer-OnHTTPRequest", $"Got HTTP {e.Request.Method.ToUpper()} -> {e.Request.Path + e.Request.QueryString}", DeltaLogLevel.Debug);
            stat_requests_handled++;

            //Check if this is a status request
            if (e.Request.Path == "/status.json")
            {
                await WriteStringToBody(e, JsonConvert.SerializeObject(GetStatus()), "application/json");
                return;
            }

            //Find a matching service
            DeltaWebServiceDefinition service = FindService(e.Request.Path);

            //Check to see if we found a service
            if(service == null)
            {
                await WriteStringToBody(e, "Not Found", "text/plain", 404);
                return;
            }

            //Parse args
            Dictionary<string, string> args = MatchWildcardArgs(e.Request.Path.ToString().Split('/'), service.GetTemplateUrl().Split('/'));

            //Create a new session
            DeltaWebService session = service.OpenRequest(conn, e);

            try
            {
                //Preauthenticate this session
                if (!await session.OnPreRequest())
                    return;

                //Set args on this session
                if (!await session.SetArgs(args))
                    return;

                //Run the actual code
                await session.OnRequest();
            } catch (Exception ex)
            {
                await WriteStringToBody(e, $"Internal Server Error. Please try again later.\n\nDEBUG DATA:\nService={service.GetType().Name}\nAppVersion={conn.system_version_major}.{conn.system_version_minor}\nLibVersion={DeltaConnection.LIB_VERSION_MAJOR}.{DeltaConnection.LIB_VERSION_MINOR}\nDeltaServerID={conn.server_id}\nRequestID={session._request_id}", "text/plain", 500);
                conn.Log("DeltaWebServer-OnHTTPRequest", $"Internal server error. Service={service.GetType().Name}, RequestID={session._request_id}, AppVersion={conn.system_version_major}.{conn.system_version_minor}, LibVersion={DeltaConnection.LIB_VERSION_MAJOR}.{DeltaConnection.LIB_VERSION_MINOR}, URL={e.Request.Path.Value}{e.Request.QueryString}, Exception={ex.Message}, StackTrace={ex.StackTrace}", DeltaLogLevel.High);
            }
        }

        /// <summary>
        /// Returns a list of headers that are exposed for CORS
        /// </summary>
        /// <returns></returns>
        private string GetExposedHeadersString()
        {
            string h = "Authorization, X-Delta-Server-ID, X-Delta-Server-Type";
            foreach (var e in exposedHeaders)
                h += ", " + e;
            return h;
        }

        /// <summary>
        /// Registers services that we can use
        /// </summary>
        /// <param name="s"></param>
        public void AddService(params DeltaWebServiceDefinition[] s)
        {
            services.AddRange(s);
        }

        /// <summary>
        /// Returns info about this server
        /// </summary>
        /// <returns></returns>
        public DeltaWebServerStatus GetStatus()
        {
            return new DeltaWebServerStatus
            {
                start = start,
                uptime = (long)(DateTime.UtcNow - start).TotalSeconds,
                enviornment = conn.config.env,
                hosts = conn.config.hosts,
                lib_version = $"{DeltaConnection.LIB_VERSION_MAJOR}.{DeltaConnection.LIB_VERSION_MINOR}",
                server_version = $"{conn.system_version_major}.{conn.system_version_minor}",
                server_id = conn.server_id,
                server_type = conn.me.server_type
            };
        }

        /// <summary>
        /// Gets the wildcard args from a request template
        /// </summary>
        /// <param name="request">Path from an active request</param>
        /// <param name="template">Path from a service's template</param>
        /// <returns></returns>
        private Dictionary<string, string> MatchWildcardArgs(string[] request, string[] template)
        {
            //Create output
            Dictionary<string, string> output = new Dictionary<string, string>();
            
            //Check for matches
            for (int i = 0; i < template.Length; i++)
            {
                //If this is not a wildcard, ignore
                if (!template[i].StartsWith('{') || !template[i].EndsWith('}'))
                    continue;

                //Add to dict
                output.Add(template[i].Trim('{').Trim('}'), request[i]);
            }

            return output;
        }

        private DeltaWebServiceDefinition FindService(string path)
        {
            //Split path data
            string[] parts = path.Split('/');

            //Match to a service
            foreach(var s in services)
            {
                if (CheckServiceMatch(parts, s))
                    return s;
            }

            return null;
        }

        private bool CheckServiceMatch(string[] parts, DeltaWebServiceDefinition s)
        {
            //Get template
            string[] template = s.GetTemplateUrl().Split('/');

            //Check length
            if (parts.Length != template.Length)
                return false;

            //Check for matches
            for (int i = 0; i < template.Length; i++)
            {
                //If this is a wildcard, ignore
                if (template[i].StartsWith('{') && template[i].EndsWith('}'))
                    continue;

                //Check for match
                if (template[i] != parts[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Writes a string to the output stream
        /// </summary>
        /// <param name="e">Context to write to</param>
        /// <param name="data">String to write</param>
        /// <param name="type">MIME type</param>
        /// <param name="code">Status code</param>
        /// <returns></returns>
        public static async Task WriteStringToBody(HttpContext e, string data, string type, int code = 200)
        {
            var response = e.Response;
            response.StatusCode = code;
            response.ContentType = type;
            var bytes = Encoding.UTF8.GetBytes(data);
            response.ContentLength = bytes.Length;
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
