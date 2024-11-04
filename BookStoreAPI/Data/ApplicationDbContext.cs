using BookStoreAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<AppUserBook> AppUserBooks { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUserBook>()
                .HasKey(ub => new { ub.UserId, ub.BookId });

            modelBuilder.Entity<AppUserBook>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.AppUserBooks)
                .HasForeignKey(ub => ub.UserId);

            modelBuilder.Entity<AppUserBook>()
                .HasOne(ub => ub.Book)
                .WithMany(b => b.AppUserBooks)
                .HasForeignKey(ub => ub.BookId);
        }
    }
}
