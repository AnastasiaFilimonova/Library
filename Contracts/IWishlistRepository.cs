using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IWishlistRepository
    {
        IEnumerable<Wishlist> GetAllWishlistItems(bool trackChanges);
        Wishlist GetWishlistItem(int bookId, bool trackChanges);
        void AddToWishlist(Wishlist item);
        void RemoveFromWishlist(Wishlist item);
    }
}
