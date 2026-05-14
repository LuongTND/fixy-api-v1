using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Helpers
{
    public static class MoMoHelper
    {
        public static string HmacSHA256(string key, string rawData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var messageBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);

            var hashBytes = hmac.ComputeHash(messageBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
