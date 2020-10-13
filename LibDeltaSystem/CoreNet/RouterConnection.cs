using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.CoreNet.IO.Client;
using LibDeltaSystem.CoreNet.IO.Transports;
using LibDeltaSystem.CoreNet.NetMessages;
using LibDeltaSystem.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.CoreNet
{
    public class RouterConnection
    {
        private ClientRouterIO io;
        private DeltaConnection conn;
        private long routerKey;
        public bool loggedIn;

        //Opcodes from servers to manager
        public const short OPCODE_SYS_LOGIN = 1;
        public const short OPCODE_SYS_GETCFG = 2;
        public const short OPCODE_SYS_USERCFG = 3;
        public const short OPCODE_SYS_LOG = 4;

        public const short OPCODE_DIST_RPC = 101;

        public RouterConnection(IPEndPoint endpoint, long routerKey, DeltaConnection conn)
        {
            //Set
            this.conn = conn;
            this.routerKey = routerKey;
            
            //Make IO
            io = new ClientRouterIO(conn, new UnencryptedTransport(), new MinorMajorVersionPair(conn.system_version_major, conn.system_version_minor), endpoint);
            io.OnConnected += Io_OnConnected;
            io.OnDisconnected += Io_OnDisconnected;
        }

        private void Io_OnConnected()
        {
            //Create login data
            byte[] sendBuffer = new byte[12];
            BitConverter.GetBytes((int)conn.server_type).CopyTo(sendBuffer, 0);
            BitConverter.GetBytes(routerKey).CopyTo(sendBuffer, 4);

            //Send
            io.SendMessage(OPCODE_SYS_LOGIN, sendBuffer);
            loggedIn = true;
        }

        private void Io_OnDisconnected()
        {
            loggedIn = false;
        }

        public async Task<LoginServerInfo> RequestConfig()
        {
            //Send and wait
            var channel = io.SendMessageGetResponseChannel(OPCODE_SYS_GETCFG, new byte[0]);
            var response = await channel.ReadAsync();
            return response.DeserializeAs<LoginServerInfo>();
        }

        public async Task<string> SendLoadUserConfigCommand(string name, string defaultValue)
        {
            //Calculate length of both name and default value
            int nameLen = Encoding.UTF8.GetByteCount(name);
            int defaultLen = Encoding.UTF8.GetByteCount(defaultValue);

            //Open buffer and write lengths
            byte[] buffer = new byte[nameLen + defaultLen + 4 + 4];
            BitConverter.GetBytes(nameLen).CopyTo(buffer, 0);
            BitConverter.GetBytes(defaultLen).CopyTo(buffer, 4 + nameLen);

            //Write strings
            Encoding.UTF8.GetBytes(name).CopyTo(buffer, 4);
            Encoding.UTF8.GetBytes(defaultValue).CopyTo(buffer, 4 + nameLen + 4);

            //Send and wait
            var channel = io.SendMessageGetResponseChannel(OPCODE_SYS_USERCFG, buffer);
            var response = await channel.ReadAsync();
            return Encoding.UTF8.GetString(response.payload);
        }

        public void SendLogCommand(string topic, string message, DeltaLogLevel level)
        {
            return;
            //Calculate lengths
            int topicLen = Encoding.UTF8.GetByteCount(topic);
            int messageLen = Encoding.UTF8.GetByteCount(message);

            //Open buffer and write
            byte[] buffer = new byte[4 + 4 + topicLen + 4 + messageLen];
            BitConverter.GetBytes((int)level).CopyTo(buffer, 0);
            BitConverter.GetBytes(topicLen).CopyTo(buffer, 4);
            Encoding.UTF8.GetBytes(topic).CopyTo(buffer, 8);
            BitConverter.GetBytes(messageLen).CopyTo(buffer, 8 + topicLen);
            Encoding.UTF8.GetBytes(message).CopyTo(buffer, 8 + topicLen + 4);

            //Send
            io.SendMessage(OPCODE_SYS_LOG, buffer);
        }

        public void SendRPCCommand(RPC.RPCOpcode opcode, byte filterType, byte[] filterData, byte actionType, byte[] actionData)
        {
            //Create buffer
            byte[] buffer = new byte[4 + 1 + 1 + filterData.Length + 1 + 4 + actionData.Length];
            BitConverter.GetBytes((int)opcode).CopyTo(buffer, 0);
            buffer[4] = filterType;
            buffer[5] = (byte)filterData.Length;
            Array.Copy(filterData, 0, buffer, 6, filterData.Length);
            buffer[6 + filterData.Length] = actionType;
            BitConverter.GetBytes(actionData.Length).CopyTo(buffer, 6 + filterData.Length + 1);
            Array.Copy(actionData, 0, buffer, 6 + filterData.Length + 5, actionData.Length);

            //Send
            io.SendMessage(OPCODE_DIST_RPC, buffer);
        }
    }
}
