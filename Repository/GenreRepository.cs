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
    public class GenreRepository : RepositoryBase<Genre>, IGenreRepository
    {
        public GenreRepository(RepositoryContext context) : base(context) { }

        public Genre GetGenreByName(string name, bool trackChanges) =>
            FindByCondition(g => g.GenreName.ToLower() == name.ToLower(), trackChanges).FirstOrDefault();

        public void CreateGenre(Genre genre) => Create(genre);
    }

}
