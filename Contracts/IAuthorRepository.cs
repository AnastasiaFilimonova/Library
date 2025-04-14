using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IAuthorRepository
    {
        Author GetAuthorByName(string authorName);
        void CreateAuthor(Author author);
    }
}
