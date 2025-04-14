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
    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(RepositoryContext repositoryContext)
        : base(repositoryContext)
        {
        }

        public Genre GetGenreByName(string genreName)
        {
            return FindByCondition(g => g.GenreName.ToLower() == genreName.ToLower(), trackChanges: false).FirstOrDefault();

        }

        public void CreateGenre(Genre genre) => Create(genre);
    }
}
