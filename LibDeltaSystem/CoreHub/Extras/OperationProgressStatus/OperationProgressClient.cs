using LibDeltaSystem.CoreHub.CoreNetwork;
using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.Extras.OperationProgressStatus
{
    public class OperationProgressClient
    {
        private ushort serverId;
        private ushort token;
        private BaseClientCoreNetwork network;
        private CoreNetworkServer server;

        public OperationProgressClient(BaseClientCoreNetwork network, uint token)
        {
            this.network = network;
            this.serverId = (ushort)((token & 0xFFFF0000) >> 16);
            this.token = (ushort)(token & 0x0000FFFF);
            server = network.list.GetServerById(serverId);
            if (server == null)
                throw new Exception("Could not find server ID requesting this!");
        }

        public void SendStatus(ushort statusCode, string msg)
        {
            byte[] data = new byte[4 + Encoding.UTF8.GetByteCount(msg)];
            BinaryTool.WriteUInt16(data, 0, token);
            BinaryTool.WriteUInt16(data, 2, statusCode);
            Encoding.UTF8.GetBytes(msg, 0, msg.Length, data, 4);
            network.SendMessage(server, CoreNetworkOpcode.OPERATION_PROGRESS_UPDATED, data);
        }
    }
}
