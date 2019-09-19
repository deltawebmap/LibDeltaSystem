using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.System
{
    public class DbToken : DbBaseSystem
    {
        /// <summary>
        /// The token string
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// The user ID this maps to
        /// </summary>
        public string user_id { get; set; }

        /// <summary>
        /// The time this token was created
        /// </summary>
        public long created_utc { get; set; }

        /// <summary>
        /// Updates this in the database
        /// </summary>
        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync()
        {
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_tokens.FindOneAndReplaceAsync(filter, this);
        }

        /// <summary>
        /// Deletes this in the database async
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync()
        {
            var filterBuilder = Builders<DbToken>.Filter;
            var filter = filterBuilder.Eq("_id", _id);
            await conn.system_tokens.FindOneAndDeleteAsync(filter);
        }

        /// <summary>
        /// Generates a secure string THAT MAY NOT BE UNIQUE.
        /// </summary>
        /// <returns></returns>
        public static string GenerateSecureString(int len)
        {
            char[] tokenChars = "1234567890ABCDEF".ToCharArray();
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[len];
            provider.GetBytes(byteArray);
            char[] outputChars = new char[len];
            for(var i = 0; i<len; i++)
            {
                char c = tokenChars[byteArray[i] % (tokenChars.Length - 1)];
                outputChars[i] = c;
            }
            return new string(outputChars);
        }
    }
}
