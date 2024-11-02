using BookStoreAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Fixtures
{
    public static class LoginFixtures
    {
        public static LoginDto GetLoginData()
        {
            return new LoginDto
            {
                Email = "test@test.com",
                Password = "password"
            };
        } 

        public static AccessTokenDto GetAccessTokenDto()
        {
            return new AccessTokenDto
            {
                AccessToken = "accesstoken",
                ExpirationDate = DateTime.Now,
            };
        }

        public static LoginResponseDto GetLoginResponsedto()
        {
            return new LoginResponseDto
            {
                AccessToken = "accesstoken",
                AccessExpirationDate = DateTime.Now,
                RefreshToken = "refreshToken",
                RefreshExpirationDate = DateTime.Now
            };
        }
    }
}
