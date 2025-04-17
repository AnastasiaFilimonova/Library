using Contracts;
using Entities;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public Author GetAuthorByName(string name, bool trackChanges)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var normalized = name.Trim().ToLower();
            return FindByCondition(a => a.AuthorName.Trim().ToLower() == normalized, trackChanges).FirstOrDefault();
        }


        public void CreateAuthor(Author author) => Create(author);

        public IQueryable<Author> FindByCondition(Expression<Func<Author, bool>> expression, bool trackChanges) =>
            base.FindByCondition(expression, trackChanges);
    }


}
