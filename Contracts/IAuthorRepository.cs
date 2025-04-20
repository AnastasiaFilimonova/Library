using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IAuthorRepository
    {
        Author GetAuthorByName(string name, bool trackChanges);
        void CreateAuthor(Author author);
        IQueryable<Author> FindByCondition(Expression<Func<Author, bool>> expression, bool trackChanges);
        Author GetOrCreateAuthor(string name);
    }

}
