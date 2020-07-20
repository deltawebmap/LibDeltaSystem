using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class PasswordTool
    {
        public static bool AuthenticateHashedPassword(string request, byte[] challengeHash, byte[] challengeSalt)
        {
            //Compute
            byte[] hash = ComputeHash(request, challengeSalt);
            return BinaryTool.CompareBytes(hash, challengeHash);
        }

        public static byte[] HashPassword(string request, out byte[] salt)
        {
            //Generate salt
            salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return ComputeHash(request, salt);
        }

        private static byte[] ComputeHash(string request, byte[] salt)
        {
            //https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-3.1
            return KeyDerivation.Pbkdf2(
            password: request,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);
        }
    }
}
