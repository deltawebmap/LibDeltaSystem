using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbMachine : DbBaseSystem
    {
        /// <summary>
        /// The type of the owner. Can be these values:
        /// "USER": A user owns this
        /// "PROVIDER": A provider owns this
        /// </summary>
        public string owner_type { get; set; }

        /// <summary>
        /// The ID of the owner
        /// </summary>
        public string owner_id { get; set; }

        /// <summary>
        /// A unique token that allows reading of this machine's data
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The short 000-000 token that is only active for a short time before the server is activated. If first_activation_time is greater than 60 seconds old, this is ignored.
        /// </summary>
        public string shorthand_token { get; set; }

        /// <summary>
        /// The name set by the user. Doesn't mean much.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The machine sent an activation signal
        /// </summary>
        public bool is_activated { get; set; }

        /// <summary>
        /// The first time this server was activated. If the server wasn't activated, this shows the time that the server was last checked for activation.
        /// </summary>
        public DateTime first_activation_time { get; set; }

        /// <summary>
        /// Latest time this server was activated
        /// </summary>
        public DateTime latest_activation_time { get; set; }

        /// <summary>
        /// The last version the server used
        /// </summary>
        public int last_version_minor { get; set; }

        /// <summary>
        /// The last version the server used
        /// </summary>
        public int last_version_major { get; set; }

        /// <summary>
        /// Creates a machine entry on the database.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="owner_type"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<DbMachine> CreateMachineAsync(DeltaConnection conn, string owner_type, ObjectId owner, string name)
        {
            //Generate a random token
            string token = Tools.SecureStringTool.GenerateSecureString(256);
            while (!await Tools.SecureStringTool.CheckStringUniquenessAsync<DbMachine>(token, conn.system_machines))
                token = Tools.SecureStringTool.GenerateSecureString(256);

            //Generate a shorthand string
            string shorthand = Tools.SecureStringTool.GenerateSecureShorthandCode();
            while (await conn.GetMachineByShorthandTokenAsync(shorthand) != null)
                shorthand = Tools.SecureStringTool.GenerateSecureShorthandCode();

            //Create an object
            DbMachine machine = new DbMachine
            {
                name = name,
                owner_id = owner.ToString(),
                owner_type = owner_type,
                token = token,
                first_activation_time = DateTime.UtcNow,
                is_activated = false,
                shorthand_token = shorthand,
                _id = ObjectId.GenerateNewId()
            };

            //Add
            await conn.system_machines.InsertOneAsync(machine);

            //Return the machine
            return machine;
        }
        
        /// <summary>
        /// Gets servers that this machine operates
        /// </summary>
        /// <returns></returns>
        public async Task<List<DbServer>> GetServersAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbServer>.Filter;
            var filter = filterBuilder.Eq("machine_uid", id);
            var results = await conn.system_servers.FindAsync(filter);
            return await results.ToListAsync();
        }

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update(DeltaConnection conn)
        {
            UpdateAsync(conn).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbMachine>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_machines.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(DeltaConnection conn)
        {
            var filterBuilder = Builders<DbMachine>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_machines.FindOneAndDeleteAsync(filter);
        }
    }
}
