using BookStoreAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Fixtures
{
    public static class RefreshFixtures
    {
        public static RefreshModel GetRefreshModel()
        {
            return new RefreshModel
            {
                AccessToken = "validAccessToken",
                RefreshToken = "validtoken"
            };
        }

        public static ClaimsPrincipal GetPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser@example.com"),
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        public static AppUser GetAppUser()
        {
            return new AppUser
            {
                RefreshToken = "validtoken",
                RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(30)
            };
        }
    }
}
