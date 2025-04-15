using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IReadingStatusRepository
    {
        IEnumerable<ReadingStatus> GetAllStatuses(bool trackChanges);
        ReadingStatus GetStatusById(int id, bool trackChanges);
        void Update(ReadingStatus status);
    }
}
