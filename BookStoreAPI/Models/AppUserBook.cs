namespace BookStoreAPI.Models
{
    public class AppUserBook
    {
        public string UserId { get; set; }
        public AppUser User { get; set; }  = new AppUser();

        public int BookId { get; set; }
        public Book Book { get; set; }  = new Book();

        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
