using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Contracts
{
    public interface IBookRepository
    {
        IEnumerable<Book> GetAllBooks(bool trackChanges);
        void CreateBook(Book book);
    }
}
