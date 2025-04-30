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
using Entities.RequestFeatures;
using X.PagedList;
using X.PagedList.Extensions;


namespace Repository
{
    public class BookRepository : RepositoryBase<Book>, IBookRepository
    {
        public BookRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
        }
        public void CreateBook(Book book) => Create(book);
        public void DeleteBook(Book book) => Delete(book);
        public void UpdateBook(Book book) => Update(book);
        public IEnumerable<Book> GetAllBooks(bool trackChanges) => RepositoryContext.Books.Include(b => b.Author).Include(b => b.Genre).Include(b => b.ReadingStatus).Include(b => b.ListBooks).ToList();
        public IPagedList<Book> GetFilteredBooks(BookParameters parameters, bool trackChanges)
        {
            var books = FindAll(trackChanges).Include(b => b.Author).Include(b => b.Genre).Include(b => b.ReadingStatus).Include(b => b.ListBooks).AsEnumerable()
                .Where(b =>
                    (string.IsNullOrEmpty(parameters.AuthorName) || b.Author.AuthorName.ToLowerInvariant().Contains(parameters.AuthorName.ToLowerInvariant())) &&
                    (string.IsNullOrEmpty(parameters.GenreName) || b.Genre.GenreName.ToLowerInvariant().Contains(parameters.GenreName.ToLowerInvariant())) &&
                    (!parameters.Year.HasValue || (b.ReadingStatus?.EndReadingDate?.Year == parameters.Year)) &&
                    (!parameters.Rating.HasValue || b.ReadingStatus?.Rating == parameters.Rating) &&
                    (!parameters.Status.HasValue || b.ReadingStatus?.Status == parameters.Status)
                )
                .ToList();
            return books.ToPagedList();
        }
    }
}
