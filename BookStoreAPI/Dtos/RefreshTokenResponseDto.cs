using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Dtos
{
    public class RefreshTokenResponseDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        [Required]
        public DateTime ExpiratonDate { get; set; }
    }
}
