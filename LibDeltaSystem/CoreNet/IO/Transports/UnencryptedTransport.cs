using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO.Transports
{
    public class UnencryptedTransport : IRouterTransport
    {
        public int DecodePacket(byte[] buffer, out RouterPacket p)
        {
            p = new RouterPacket();
            int consumed = p.Deserialize(buffer);
            return consumed;
        }

        public int EncodePacket(byte[] buffer, RouterPacket p)
        {
            int len = p.GetLength();
            p.Serialize(buffer, 0);
            return len;
        }

        public int GetFrameSize()
        {
            return BaseRouterIO.MESSAGE_TOTAL_SIZE;
        }
    }
}
