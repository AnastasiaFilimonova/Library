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
        public IEnumerable<Book> GetAllBooks(bool trackChanges) => FindAll(trackChanges).OrderBy(c => c.Title).ToList();
        public void CreateBook(Book book) => Create(book);
        public IQueryable<Book> FindByCondition(Expression<Func<Book, bool>> expression, bool trackChanges)
        {
            return trackChanges
                ? RepositoryContext.Books.Where(expression)
                : RepositoryContext.Books.AsNoTracking().Where(expression);
        }

    }
}
