using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IGenreRepository
    {
        Genre GetGenreByName(string name, bool trackChanges);
        void CreateGenre(Genre genre);
    }

}
