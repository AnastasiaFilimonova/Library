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
    public class ListBookRepository: RepositoryBase<ListBook>, IListBookRepository
    {
        public ListBookRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
        public bool Exists(int userId, int bookId)
        {
            return RepositoryContext.ListBooks.Any(lb => lb.UserID == userId && lb.BookID == bookId);
        }
        public void Create(ListBook listBook)
        {
            RepositoryContext.ListBooks.Add(listBook);
        }
    }
}
