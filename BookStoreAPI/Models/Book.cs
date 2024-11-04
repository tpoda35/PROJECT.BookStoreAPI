namespace BookStoreAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Pages { get; set; }

        public ICollection<AppUserBook> AppUserBooks { get; set; } = new List<AppUserBook>();
    }
}
