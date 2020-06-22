using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets
{
    public abstract class DeltaWebSocketService : DeltaWebService
    {
        public WebSocket sock;
        
        public DeltaWebSocketService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            if (e.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await e.WebSockets.AcceptWebSocketAsync();
                await OnAcceptSocket(webSocket);
            }
            else
            {
                await WriteString("Expected WebSocket to this endpoint.", "text/plain", 400);
            }
        }

        private async Task OnAcceptSocket(WebSocket socket)
        {
            //Run the opened function
            this.sock = socket;
            await OnSockOpened(socket);

            try
            {
                //Go into download loop
                byte[] buffer = new byte[4096];
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    if (result.EndOfMessage)
                    {
                        //This is a complete message that fit in a single buffer.
                        await OnReceiveData(buffer, result.Count, result.MessageType);
                    }
                    else
                    {
                        //This is a multipart message
                        await ReceiveMultipart(socket, buffer, result);
                    }
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            } catch (Exception ex)
            {
                Log("DISCONNECT", "Disconnected socket because of an error in the download loop: "+ex.Message + ex.StackTrace, ConsoleColor.Red);
                try
                {
                    await sock.CloseAsync(WebSocketCloseStatus.InternalServerError, "INTERNAL_DELTA_SERVER_ERROR", CancellationToken.None);
                }
                catch { }
            }

            //Send closed message
            await OnSockClosed(socket);
        }

        /// <summary>
        /// Handles getting multipart messages
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="buffer"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ReceiveMultipart(WebSocket sock, byte[] buffer, WebSocketReceiveResult result)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                //Write existing buffer
                ms.Write(buffer, 0, buffer.Length);

                //Receive until end
                result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (true)
                {
                    ms.Write(buffer, 0, result.Count);
                    Log("MULTIPART", $"Got {result.Count} bytes, {ms.Length} total bytes, EOM={result.EndOfMessage.ToString()}");
                    if (!result.EndOfMessage)
                        result = await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    else
                        break;
                }

                //Finalize
                await ms.FlushAsync();
                await OnReceiveData(ms.ToArray(), (int)ms.Length, result.MessageType);
            }
        }

        private async Task OnReceiveData(byte[] data, int length, WebSocketMessageType type)
        {
            if(type == WebSocketMessageType.Text)
            {
                await OnReceiveText(Encoding.UTF8.GetString(data, 0, length));
            } else if (type == WebSocketMessageType.Binary)
            {
                await OnReceiveBinary(data, length);
            }
        }

        public abstract Task OnSockOpened(WebSocket sock);

        public abstract Task OnSockClosed(WebSocket sock);

        public abstract Task OnReceiveBinary(byte[] data, int length);

        public abstract Task OnReceiveText(string data);

        public async Task SendData(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            await sock.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendData(byte[] buffer)
        {
            await sock.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public async Task SendData(byte[] buffer, int length, WebSocketMessageType type)
        {
            await sock.SendAsync(new ArraySegment<byte>(buffer, 0, length), type, true, CancellationToken.None);
        }

        public async Task DisconnectAsync(WebSocketCloseStatus status, string reason)
        {
            await sock.CloseAsync(status, reason, CancellationToken.None);
        }
    }
}
