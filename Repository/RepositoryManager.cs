using Contracts;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private RepositoryContext _repositoryContext;
        private IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IGenreRepository _genreRepository;
        private IReadingStatusRepository _readingStatusRepository;
        public RepositoryManager(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
            _bookRepository = new BookRepository(repositoryContext);
            _authorRepository = new AuthorRepository(repositoryContext);
            _genreRepository = new GenreRepository(repositoryContext);
            _readingStatusRepository = new ReadingStatusRepository(repositoryContext);
        }
        public IBookRepository Book
        {
            get
            {
                if (_bookRepository == null)
                    _bookRepository = new BookRepository(_repositoryContext);
                return _bookRepository;
            }
        }
        public IAuthorRepository Author => _authorRepository;
        public IGenreRepository Genre => _genreRepository;
        public IReadingStatusRepository ReadingStatus => _readingStatusRepository ??= new ReadingStatusRepository(_repositoryContext);

        public IWishlistRepository Wishlist => _wishlist ??= new WishlistRepository(_repositoryContext);
        private IWishlistRepository _wishlist;

        public void Save() => _repositoryContext.SaveChanges();
    }
}

    

