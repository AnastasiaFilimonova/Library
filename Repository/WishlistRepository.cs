using Contracts;
using Entities;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class WishlistRepository : RepositoryBase<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(RepositoryContext context) : base(context) { }

        public IEnumerable<Wishlist> GetAllWishlistItems(bool trackChanges) =>
            FindAll(trackChanges).Include(w => w.Book).ThenInclude(b => b.Author)
                                 .Include(w => w.Book.Genre)
                                 .ToList();

        public Wishlist GetWishlistItem(int bookId, bool trackChanges) =>
            FindByCondition(w => w.BookID == bookId, trackChanges)
            .Include(w => w.Book)
            .ThenInclude(b => b.Author)
            .Include(w => w.Book.Genre)
            .FirstOrDefault();

        public void AddToWishlist(Wishlist item) => Create(item);

        public void RemoveFromWishlist(Wishlist item) => Delete(item);
    }
}
