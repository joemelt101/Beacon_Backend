using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Beacon.Server.Controllers
{
    public static class Helper
    {
        public static String GenerateSalt()
        {
            Random randomByteGenerator = new Random();
            byte[] saltAsByteArray = new byte[20];
            randomByteGenerator.NextBytes(saltAsByteArray);
            return BitConverter.ToString(saltAsByteArray).Replace("-", "");
        }

        public static String HashPassword(string password, string salt)
        {
            byte[] combinedBytes = System.Text.Encoding.ASCII.GetBytes(password + salt);
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(combinedBytes);
            string hashedPassword = BitConverter.ToString(hash).Replace("-", "");
            return hashedPassword;
        }
    }
}
