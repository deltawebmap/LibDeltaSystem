using LibDeltaSystem.Entities.MiscNet;
using LibDeltaSystem.WebFramework.Entities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework
{
    /// <summary>
    /// The actual executable for the service. New object is created each time a request is to be handled
    /// </summary>
    public abstract class DeltaWebService
    {
        public DeltaConnection conn;
        public HttpContext e;
        public string method;

        private DateTime start;
        private DateTime checkpoint;
        private int checkpoint_index;
        private string checkpoint_name;

        private int _request_id; //Random request ID for logging

        public DeltaWebService(DeltaConnection conn, HttpContext e)
        {
            this.e = e;
            this.conn = conn;
            this.method = e.Request.Method.ToUpper();
            _request_id = new Random().Next();
            start = DateTime.UtcNow;
            checkpoint = start;
            checkpoint_index = 0;
            checkpoint_name = "Default";
        }

        /// <summary>
        /// Called before args are created. Do authorization here. Return false to fail
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> OnPreRequest();

        /// <summary>
        /// Sets args that were passed via URL. Keys are defined in the definition
        /// </summary>
        /// <param name="args"></param>
        public abstract Task<bool> SetArgs(Dictionary<string, string> args);

        /// <summary>
        /// Handles the actual request
        /// </summary>
        /// <returns></returns>
        public abstract Task OnRequest();

        /// <summary>
        /// Writes a string to the output stream
        /// </summary>
        /// <param name="data">String to write</param>
        /// <param name="type">MIME type</param>
        /// <param name="code">Status code</param>
        /// <returns></returns>
        public async Task WriteString(string data, string type, int code = 200)
        {
            EndDebugCheckpoint("Output Writing");
            var response = e.Response;
            response.StatusCode = code;
            response.ContentType = type;
            var bytes = Encoding.UTF8.GetBytes(data);
            response.ContentLength = bytes.Length;
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes JSON to the output stream
        /// </summary>
        /// <typeparam name="T">Type of data to write</typeparam>
        /// <param name="data">Data to write</param>
        /// <param name="code">Status code</param>
        /// <returns></returns>
        public async Task WriteJSON<T>(T data, int code = 200)
        {
            //Serialize
            string s = JsonConvert.SerializeObject(data);

            //Write
            await WriteString(s, "application/json", code);
        }

        /// <summary>
        /// Writes a basic JSON object with the OK status
        /// </summary>
        /// <param name="ok"></param>
        /// <returns></returns>
        public async Task WriteStatus(bool ok)
        {
            await WriteJSON(new OkStatusResponse
            {
                ok = ok
            });
        }
        
        /// <summary>
        /// Decodes the request data
        /// </summary>
        /// <typeparam name="T">The type of data to serialize to</typeparam>
        /// <returns></returns>
        public async Task<T> DecodePOSTBody<T>()
        {
            //Read stream
            string buffer;
            using (StreamReader sr = new StreamReader(e.Request.Body))
                buffer = await sr.ReadToEndAsync();

            //Assume this is JSON
            return JsonConvert.DeserializeObject<T>(buffer);
        }

        /// <summary>
        /// Ends the laest checkpoint and logs data if debug mode is on
        /// </summary>
        public void EndDebugCheckpoint(string name)
        {
            if (!conn.debug_mode)
                return;
            e.Response.Headers.Add("X-DeltaDebugCheckpoint-" + checkpoint_index, $"{checkpoint_name} / {Math.Round((DateTime.UtcNow - checkpoint).TotalMilliseconds)}ms / {Math.Round((DateTime.UtcNow - start).TotalMilliseconds)}ms");
            checkpoint_index++;
            checkpoint = DateTime.UtcNow;
            checkpoint_name = name;
        }

        public void Log(string topic, string msg)
        {
            if (conn.debug_mode)
                Console.WriteLine($"[DeltaWebService@{_request_id}: {topic}] {msg}");
        }

        public DeltaCommonHTTPMethod GetMethod()
        {
            if (Enum.TryParse<DeltaCommonHTTPMethod>(e.Request.Method.ToUpper(), out DeltaCommonHTTPMethod method))
                return method;
            return DeltaCommonHTTPMethod.Unknown;
        }
    }
}
