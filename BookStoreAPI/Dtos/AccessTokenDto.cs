namespace BookStoreAPI.Dtos
{
    public class AccessTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }
}
