using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Tools
{
    public static class SecureStringTool
    {
        private static RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a secure string THAT MAY NOT BE UNIQUE.
        /// </summary>
        /// <returns></returns>
        public static string GenerateSecureString(int len)
        {
            char[] tokenChars = "1234567890ABCDEF".ToCharArray();
            var byteArray = new byte[len];
            provider.GetBytes(byteArray);
            char[] outputChars = new char[len];
            for (var i = 0; i < len; i++)
            {
                char c = tokenChars[byteArray[i] % (tokenChars.Length - 1)];
                outputChars[i] = c;
            }
            return new string(outputChars);
        }

        /// <summary>
        /// Checks against a collection if this string was already in use. Returns true if this string is unique.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<bool> CheckStringUniquenessAsync<T>(string token, IMongoCollection<T> collec, string index_name = "token")
        {
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq(index_name, token);
            var results = await collec.FindAsync(filter);
            return await results.FirstOrDefaultAsync() == null;
        }
    }
}
