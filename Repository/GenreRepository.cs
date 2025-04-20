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
    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(RepositoryContext context) : base(context) { }

        public Genre GetGenreByName(string name, bool trackChanges)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var normalized = NormalizeName(name);
            return FindByCondition(g => g.GenreName == normalized, trackChanges)
                .FirstOrDefault();
        }


        public Genre GetOrCreateGenre(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var normalized = name.Trim().ToLower();
            var genre = FindByCondition(g => g.GenreName.ToLower().Trim() == normalized, false)
                .FirstOrDefault();

            if (genre != null)
                return genre;

            var newGenre = new Genre { GenreName = NormalizeName(name) };
            Create(newGenre);
            RepositoryContext.SaveChanges(); // << важно сохранить, чтобы получить Id

            return newGenre;
        }

        public void CreateGenre(Genre genre)
        {
            genre.GenreName = NormalizeName(genre.GenreName);
            Create(genre);
        }

        private string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            input = input.Trim().ToLower();
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        //public IQueryable<Genre> FindByCondition(Expression<Func<Genre, bool>> expression, bool trackChanges) =>
        //    base.FindByCondition(expression, trackChanges);
    }
}
