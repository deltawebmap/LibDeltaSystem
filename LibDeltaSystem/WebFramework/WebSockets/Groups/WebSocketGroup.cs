using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.WebFramework.WebSockets.Groups
{
    public class WebSocketGroup
    {
        public readonly WebSocketGroupQuery identifier;
        private readonly List<GroupWebSocketService> clients;

        public WebSocketGroup(WebSocketGroupQuery identifier)
        {
            this.identifier = identifier;
            this.clients = new List<GroupWebSocketService>();
        }

        /// <summary>
        /// Checks if a new client should be added to this group. Checks against the identifier
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public bool CheckIfAuthorized(WebSocketGroupQuery q)
        {
            return identifier.CheckIfAuthorized(q);
        }

        public void AddClient(GroupWebSocketService client)
        {
            lock (clients)
                clients.Add(client);
            client.group = this;
        }

        public void RemoveClient(GroupWebSocketService client)
        {
            lock (clients)
                clients.Remove(client);
            client.group = null;
        }

        public int GetClientCount()
        {
            return clients.Count;
        }

        /// <summary>
        /// Sends a message to all clients
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task SendDistributedMessage(byte[] data, int length, WebSocketMessageType type, List<GroupWebSocketService> ignoredClients)
        {
            //Start tasks
            List<Task> tasks = new List<Task>();
            lock(clients)
            {
                foreach(var c in clients)
                {
                    if (ignoredClients.Contains(c))
                        continue;
                    tasks.Add(c.SendData(data, length, type));
                }
            }

            //Wait for tasks
            await Task.WhenAll(tasks);
        }
    }
}
