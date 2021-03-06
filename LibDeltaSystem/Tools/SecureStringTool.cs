﻿using MongoDB.Driver;
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
            return GenerateSecureString(len, "1234567890ABCDEF".ToCharArray());
        }

        /// <summary>
        /// Generates a secure string THAT MAY NOT BE UNIQUE. This will use 0-9, A-Z
        /// </summary>
        /// <returns></returns>
        public static string GenerateSecureStringFullCaps(int len)
        {
            return GenerateSecureString(len, "1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
        }

        /// <summary>
        /// Generates a secure string THAT MAY NOT BE UNIQUE.
        /// </summary>
        /// <returns></returns>
        public static string GenerateSecureShorthandCode()
        {
            return GenerateSecureString(8, "01234567890".ToCharArray());
        }

        /// <summary>
        /// Generates a secure string THAT MAY NOT BE UNIQUE.
        /// </summary>
        /// <returns></returns>
        public static string GenerateSecureString(int len, char[] charset)
        {
            var byteArray = new byte[len];
            provider.GetBytes(byteArray);
            char[] outputChars = new char[len];
            for (var i = 0; i < len; i++)
            {
                char c = charset[byteArray[i] % (charset.Length - 1)];
                outputChars[i] = c;
            }
            return new string(outputChars);
        }

        /// <summary>
        /// Generates secure random bytes
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] GenerateSecureRandomBytes(int len)
        {
            byte[] buf = new byte[len];
            provider.GetBytes(buf);
            return buf;
        }

        public static int GenerateSecureInteger()
        {
            byte[] buf = new byte[4];
            provider.GetBytes(buf);
            return BitConverter.ToInt32(buf, 0);
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
