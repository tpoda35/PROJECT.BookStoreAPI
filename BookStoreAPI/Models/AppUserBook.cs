namespace BookStoreAPI.Models
{
    public class AppUserBook
    {
        public string UserId { get; set; }
        public AppUser? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
