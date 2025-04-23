using Entities.RequestFeatures;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Contracts
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAllBooks(bool trackChanges);
        void CreateBook(Book book);
        IQueryable<Book> FindByCondition(Expression<Func<Book, bool>> expression, bool trackChanges);
        void DeleteBook(Book book);
        void UpdateBook(Book book);
        IPagedList<Book> GetFilteredBooks(BookParameters bookParams, bool trackChanges);
    }
}

