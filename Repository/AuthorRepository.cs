using Contracts;
using Entities;
using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class AuthorRepository : RepositoryBase<Author>, IAuthorRepository
    {
        public AuthorRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
        }

        public Author GetAuthorByName(string authorName)
        {
            return FindByCondition(a => a.AuthorName.ToLower() == authorName.ToLower(), trackChanges: false).FirstOrDefault();

        }


        public void CreateAuthor(Author author) => Create(author);
    }
}
