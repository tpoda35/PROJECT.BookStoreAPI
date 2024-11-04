using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Dtos
{
    public class RefreshModel
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
