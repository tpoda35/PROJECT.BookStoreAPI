using System.Security.Cryptography;

namespace BookStoreAPI.Helpers
{
    public static class RefreshTokenHelper
    {
        public static string GenerateRefreshToken()
        {
            var rndNum = new byte[64];
            var generator = RandomNumberGenerator.Create();

            generator.GetBytes(rndNum);

            return Convert.ToBase64String(rndNum);
        }
    }
}
