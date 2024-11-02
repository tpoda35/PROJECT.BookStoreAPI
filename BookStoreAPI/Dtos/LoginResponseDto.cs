using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessExpirationDate { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshExpirationDate { get; set; }
    }
}
