using Konscious.Security.Cryptography;
using PSKVideoProjectBackend.Models;
using System.Security.Cryptography;
using System.Text;

namespace PSKVideoProjectBackend.Helpers
{
    public static class HashHelpers
    {
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // 16 bytes = 128 bits salt

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public static bool VerifyPassword(string password, byte[] salt, byte[] hash)
        {
            byte[] newHash = HashPassword(password, salt);
            return newHash.SequenceEqual(hash);
        }

        public static byte[] HashPassword(this string password, byte[] salt)
        {
            //we use static iterations count and length (4, 32)
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) {
                Salt = salt,
                DegreeOfParallelism = 8, // number of threads
                Iterations = 4,
                MemorySize = 65536,
            };

            return argon2.GetBytes(32);
        }
    }
}
