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
            if (string.IsNullOrWhiteSpace(name)) return null;

            var normalized = NormalizeName(name); // нормализуем заранее
            return FindByCondition(a => a.AuthorName == normalized, trackChanges)
                .FirstOrDefault();
        }




        public void CreateAuthor(Author author)
        {
            author.AuthorName = NormalizeName(author.AuthorName);
            Create(author);
        }

        public Author GetOrCreateAuthor(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var normalized = name.Trim().ToLower();

            var author = FindByCondition(a => a.AuthorName.ToLower().Trim() == normalized, false)
                .FirstOrDefault();

            if (author != null)
                return author;

            // Создаём, если не найден
            var newAuthor = new Author { AuthorName = NormalizeName(name) };
            Create(newAuthor);
            RepositoryContext.SaveChanges(); // << ВАЖНО!

            return newAuthor;
        }

        private string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.Trim().ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        //public IQueryable<Author> FindByCondition(Expression<Func<Author, bool>> expression, bool trackChanges) =>
        //    base.FindByCondition(expression, trackChanges);
    }
}
