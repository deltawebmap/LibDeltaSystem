using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    /// <summary>
    /// Represents a token that is pending creation
    /// </summary>
    public class DbPreflightToken : DbBaseSystem
    {
        /// <summary>
        /// The token to return
        /// </summary>
        public string final_token { get; set; }

        /// <summary>
        /// The token that will be used to get this
        /// </summary>
        public string preflight_token { get; set; }

        /// <summary>
        /// The time this was created
        /// </summary>
        public DateTime creation { get; set; }

        /// <summary>
        /// A random token submitted by the client
        /// </summary>
        public int nonce { get; set; }

        /// <summary>
        /// Where to head next after auth, usually an endpoint that will then request this object
        /// </summary>
        public int redirect_type { get; set; }

        /// <summary>
        /// Has this token been authorized with Steam yet?
        /// </summary>
        public bool auth { get; set; }

        /// <summary>
        /// Where to go after this
        /// </summary>
        public string next { get; set; }

        /// <summary>
        /// Authenticates this preflight by generating a token
        /// </summary>
        /// <returns></returns>
        public async Task SetUser(DeltaConnection conn, DbUser user)
        {
            //Generate a token
            var token = await user.MakeToken(conn);

            //Create updates
            var builder = Builders<DbPreflightToken>.Update;
            var update = builder.Set("auth", true).Set("final_token", token);

            //Apply
            await DeltaConnection.UpdateDocumentById(conn.system_preflight_tokens, _id, update);
        }
    }
}
