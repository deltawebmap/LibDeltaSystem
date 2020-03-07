using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.Groups
{
    public abstract class GroupWebSocketService : DeltaWebSocketService
    {
        public WebSocketGroupHolder holder;
        public WebSocketGroup group;
        
        public GroupWebSocketService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        /// <summary>
        /// Sends a message to all clients in our group
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task SendMessageToGroup(byte[] data, int length, WebSocketMessageType type)
        {
            if (group == null)
                return;
            await group.SendDistributedMessage(data, length, type, new List<GroupWebSocketService>
            {
                this
            });
        }

        /// <summary>
        /// Gets the group holder
        /// </summary>
        /// <returns></returns>
        public abstract WebSocketGroupHolder GetGroupHolder();

        /// <summary>
        /// Authenticates a group query
        /// </summary>
        /// <returns></returns>
        public abstract Task<WebSocketGroupQuery> AuthenticateGroupQuery();

        public override async Task OnSockOpened(WebSocket sock)
        {
            //Get our group holder
            holder = GetGroupHolder();

            //Now, authenticate our query
            WebSocketGroupQuery query = await AuthenticateGroupQuery();

            //Locate a group for us to use
            group = holder.AddClient(this, query);
        }

        public override async Task OnSockClosed(WebSocket sock)
        {
            //Remove ourselves from this group
            if(group != null)
            {
                holder.RemoveClient(this);
            }
        }
    }
}
