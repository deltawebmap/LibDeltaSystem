using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreHub.Entities
{
    public class CoreStatusResponse
    {
        public byte libVersionMajor;
        public byte libVersionMinor;
        public byte appVersionMajor;
        public byte appVersionMinor;
        public ushort serverId;
        public byte serverType;
        public byte serverStatus;
        public TimeSpan serverUptime;
        public DateTime serverTime;
        public PlatformID serverPlatform;
        public string serverMachineName;

        public CoreStatusResponse(byte[] payload)
        {
            libVersionMajor = payload[0];
            libVersionMinor = payload[1];
            appVersionMajor = payload[2];
            appVersionMinor = payload[3];
            serverId = Tools.BinaryTool.ReadUInt16(payload, 4);
            serverType = payload[6];
            serverStatus = payload[7];
            serverUptime = new TimeSpan(0, 0, Tools.BinaryTool.ReadInt32(payload, 8));
            serverTime = new DateTime(Tools.BinaryTool.ReadInt64(payload, 12));
            serverPlatform = (PlatformID)payload[20];
            serverMachineName = Encoding.UTF8.GetString(payload, 22, payload[21]);
        }
    }
}
