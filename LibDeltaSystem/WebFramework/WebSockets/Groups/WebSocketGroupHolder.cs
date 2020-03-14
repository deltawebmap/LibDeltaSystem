using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.Groups
{
    public class WebSocketGroupHolder
    {
        private readonly List<WebSocketGroup> groups;

        public WebSocketGroupHolder()
        {
            this.groups = new List<WebSocketGroup>();
        }

        /// <summary>
        /// Creates a new group and adds it to our list of groups
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private WebSocketGroup CreateGroup(WebSocketGroupQuery query)
        {
            WebSocketGroup g = new WebSocketGroup(query);
            groups.Add(g);
            return g;
        }

        /// <summary>
        /// Removes an empty group.
        /// </summary>
        /// <param name="group"></param>
        private void RemoveGroup(WebSocketGroup group)
        {
            groups.Remove(group);
        }

        /// <summary>
        /// Searches to see if a query can find a group. May return null.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public WebSocketGroup FindGroup(WebSocketGroupQuery query)
        {
            foreach (var g in groups)
            {
                if (g.CheckIfAuthorized(query))
                    return g;
            }
            return null;
        }

        /// <summary>
        /// Gets or creates a group for a client
        /// </summary>
        /// <param name="query"></param>
        public WebSocketGroup GetClientGroup(WebSocketGroupQuery query)
        {
            lock(groups)
            {
                //Search for groups that will accept this
                WebSocketGroup g = FindGroup(query);
                if (g != null)
                    return g;

                //We'll need to create a group
                return CreateGroup(query);
            }
        }

        /// <summary>
        /// Adds a client to a group
        /// </summary>
        /// <param name="query"></param>
        public WebSocketGroup AddClient(GroupWebSocketService client, WebSocketGroupQuery query)
        {
            //Get a group for this client
            WebSocketGroup g = GetClientGroup(query);

            //Add client to group
            g.AddClient(client);

            return g;
        }

        public void RemoveClient(GroupWebSocketService client)
        {
            while(client.groups.Count > 0)
            {
                //Get group
                var g = client.groups[0];
                
                //Remove the client from the group
                g.RemoveClient(client);

                //If this group is empty, remove it
                if (g.GetClientCount() == 0)
                    RemoveGroup(g);
            }
        }

        /// <summary>
        /// Sends a message to a query and returns if a group was found for it
        /// </summary>
        /// <param name="query"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> DistributeMessage(WebSocketGroupQuery query, byte[] data, int length, WebSocketMessageType type)
        {
            //Try to find a valid group
            WebSocketGroup g = FindGroup(query);
            if (g == null)
                return false;

            //Send message
            await g.SendDistributedMessage(data, length, type, new List<GroupWebSocketService>());
            return true;
        }
    }
}
