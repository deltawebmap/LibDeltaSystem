using LibDeltaSystem.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.IO.Server
{
    public interface IServerRouterIO : IDeltaLogger
    {
        IRouterTransport GetTransport();
        MinorMajorVersionPair GetAppVersion();
        void OnIncomingClientMessage(ServerRouterSession client, RouterMessage msg);
        void SendClientPacket(ServerRouterSession client, RouterPacket msg);
        void DropClient(ServerRouterSession clientIn);
    }
}
