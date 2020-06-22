using LibDeltaSystem.Db.Content;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools
{
    public static class SyncCommandTool
    {
        public static async Task<ObjectId> _SendCommand(DeltaConnection conn, int opcode, bool persist, ObjectId server_id, ObjectId sender_id, object payload)
        {
            ObjectId id = ObjectId.GenerateNewId();
            await conn.system_queued_sync_commands.InsertOneAsync(new Db.System.DbQueuedSyncRequest
            {
                opcode = opcode,
                payload = JsonConvert.SerializeObject(payload),
                persist = persist,
                server_id = server_id,
                sender_id = sender_id,
                _id = id
            });
            return id;
        }

        public static async Task RequestConsoleLog(DeltaConnection conn, ObjectId server_id, ObjectId sender_id, string text)
        {
            //Create payload
            JObject payload = new JObject();
            payload["text"] = text;

            //Send
            await _SendCommand(conn, -1, false, server_id, sender_id, payload);
        }

        public static async Task RequestDinoUpdate(DeltaConnection conn, ObjectId server_id, ObjectId sender_id, ulong id)
        {
            //Split ID back into multipart
            byte[] buf = BitConverter.GetBytes(id);
            int id1 = BitConverter.ToInt32(buf, 0);
            int id2 = BitConverter.ToInt32(buf, 4);

            //Create payload
            JObject payload = new JObject();
            payload["dino_id_1"] = id1;
            payload["dino_id_2"] = id2;

            //Send
            await _SendCommand(conn, 1, false, server_id, sender_id, payload);
        }

        public static async Task RequestDinoUpdate(DeltaConnection conn, DbDino dino, ObjectId sender_id)
        {
            await RequestDinoUpdate(conn, dino.server_id, sender_id, dino.dino_id);
        }
    }
}
