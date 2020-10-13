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

        public int _request_id; //Random request ID for logging

        public DeltaWebService(DeltaConnection conn, HttpContext e)
        {
            this.e = e;
            this.conn = conn;
            this.method = e.Request.Method.ToUpper();
            _request_id = new Random().Next();
            start = DateTime.UtcNow;
        }

        /// <summary>
        /// Called before args are created. Do authorization here. Return false to fail
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> OnPreRequest()
        {
            return true;
        }

        /// <summary>
        /// Sets args that were passed via URL. Keys are defined in the definition
        /// </summary>
        /// <param name="args"></param>
        public virtual async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            return true;
        }

        /// <summary>
        /// Handles the actual request
        /// </summary>
        /// <returns></returns>
        public abstract Task OnRequest();

        private bool stringHeadersWritten = false;

        /// <summary>
        /// Writes a string to the output stream
        /// </summary>
        /// <param name="data">String to write</param>
        /// <param name="type">MIME type</param>
        /// <param name="code">Status code</param>
        /// <returns></returns>
        public async Task WriteString(string data, string type, int code = 200)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var response = e.Response;
            if(!stringHeadersWritten)
            {
                response.StatusCode = code;
                response.ContentType = type;
                stringHeadersWritten = true;
            }
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
        /// Attemps to read POST content and writes errors if it fails. Returns null if failed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ReadPOSTContentChecked<T>(string method = "POST")
        {
            if (e.Request.Method.ToUpper() != method)
            {
                await WriteString("Only "+method+" requests are allowed here.", "text/plain", 400);
                return default(T);
            }
            T request = await DecodePOSTBody<T>();
            if (request == null)
            {
                await WriteString("No " + method + " body provided.", "text/plain", 400);
                return default(T);
            }
            return request;
        }

        public void Log(string topic, string msg, ConsoleColor color = ConsoleColor.White)
        {
            conn.Log("DeltaWebService-" + topic, msg, DeltaLogLevel.Low);
        }

        public DeltaCommonHTTPMethod GetMethod()
        {
            if (Enum.TryParse<DeltaCommonHTTPMethod>(e.Request.Method.ToUpper(), out DeltaCommonHTTPMethod method))
                return method;
            return DeltaCommonHTTPMethod.Unknown;
        }

        public int GetIntFromQuery(string name, int defaultValue, int min, int max)
        {
            int value = GetIntFromQuery(name, defaultValue);
            if (value > max)
                value = max;
            if (value < min)
                value = min;
            return value;
        }

        public int GetIntFromQuery(string name, int defaultValue)
        {
            if (!e.Request.Query.ContainsKey(name))
                return defaultValue;
            if (int.TryParse(e.Request.Query[name], out int value))
                return value;
            else
                return defaultValue;
        }

        public bool TryGetIntFromQuery(string name, out int value)
        {
            value = 0;
            if (!e.Request.Query.ContainsKey(name))
                return false;
            return int.TryParse(e.Request.Query[name], out value);
        }

        public Task AwaitCancel()
        {
            //Little bit janky...
            var promise = new TaskCompletionSource<bool>();
            e.RequestAborted.Register(() =>
            {
                promise.SetResult(true);
            });
            return promise.Task;
        }

        public void Redirect(string path, bool forever = false)
        {
            //Make sure we haven't started
            if (stringHeadersWritten)
                throw new Exception("Cannot redirect. Headers have already been written.");

            //Redirect
            stringHeadersWritten = true;
            e.Response.Redirect(path, forever);
        }
    }
}
