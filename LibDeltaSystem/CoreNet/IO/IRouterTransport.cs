using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO
{
    public interface IRouterTransport
    {
        /// <summary>
        /// Returns the max buffer size to allocate
        /// </summary>
        /// <returns></returns>
        int GetFrameSize();

        /// <summary>
        /// Decodes a packet and returns the number of bytes consumed
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        int DecodePacket(byte[] buffer, out RouterPacket packet);

        /// <summary>
        /// Encodes a packet and returns the number of bytes used
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        int EncodePacket(byte[] buffer, RouterPacket packet);
    }
}
