using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Helpers
{
    public static class VnPayHelper
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new HMACSHA512(Encoding.UTF8.GetBytes(key));

            var result = hash.ComputeHash(Encoding.UTF8.GetBytes(inputData));

            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }
    }
}
