using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<ListBook> ListBooks { get; set; }
        public DbSet<ReadingStatus> ReadingStatuses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ListBook>().HasKey(lb => new { lb.UserID, lb.BookID });
            modelBuilder.Entity<ListBook>().HasOne(lb => lb.User).WithMany(u => u.ListBooks).HasForeignKey(lb => lb.UserID);
            modelBuilder.Entity<ListBook>().HasOne(lb => lb.Book).WithMany(b => b.ListBooks).HasForeignKey(lb => lb.BookID);

            modelBuilder.Entity<Wishlist>().HasKey(w => new { w.UserID, w.BookID });
            modelBuilder.Entity<Wishlist>().HasOne(w => w.User).WithMany(u => u.Wishlist).HasForeignKey(w => w.UserID);
            modelBuilder.Entity<Wishlist>().HasOne(w => w.Book).WithMany(b => b.Wishlist).HasForeignKey(w => w.BookID);

            modelBuilder.Entity<Author>().HasIndex(a => a.AuthorName).IsUnique();
            modelBuilder.Entity<Genre>().HasIndex(g => g.GenreName).IsUnique();
        }
    }
}
