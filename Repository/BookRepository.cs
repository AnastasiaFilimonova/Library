using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Contracts;
using Library.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class BookRepository : RepositoryBase<Book>, IBookRepository
    {
        public BookRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
        }
        
        public void CreateBook(Book book) => Create(book);
        public void DeleteBook (Book book) => Delete(book);
        public IEnumerable<Book> GetAllBooks(bool trackChanges) =>
    RepositoryContext.Books
        .Include(b => b.Author)
        .Include(b => b.Genre)
        .Include(b => b.ReadingStatus)
        .ToList();




    }
}
