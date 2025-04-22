using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IRepositoryManager
    {
        IBookRepository Book { get; }
        IAuthorRepository Author { get; }
        IGenreRepository Genre { get; }
        IReadingStatusRepository ReadingStatus { get; }
        IWishlistRepository Wishlist { get; }
        IListBookRepository ListBook { get; }
        void Save();
    }
}
