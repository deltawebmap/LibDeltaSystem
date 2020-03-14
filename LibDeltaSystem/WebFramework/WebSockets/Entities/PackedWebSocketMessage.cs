using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace LibDeltaSystem.WebFramework.WebSockets.Entities
{
    public struct PackedWebSocketMessage
    {
        public byte[] data;
        public int length;
        public WebSocketMessageType type;

        public PackedWebSocketMessage(byte[] data, int length, WebSocketMessageType type)
        {
            this.data = data;
            this.length = length;
            this.type = type;
        }
    }
}
