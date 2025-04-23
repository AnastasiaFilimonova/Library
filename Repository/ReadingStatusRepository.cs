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
    public class ReadingStatusRepository : RepositoryBase<ReadingStatus>, IReadingStatusRepository
    {
        public ReadingStatusRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }
        public IEnumerable<ReadingStatus> GetAllStatuses(bool trackChanges) => FindAll(trackChanges).ToList();
        public ReadingStatus GetStatusById(int id, bool trackChanges) => FindByCondition(rs => rs.Id == id, trackChanges).FirstOrDefault();
        public void Update(ReadingStatus status) => base.Update(status);
        public void CreateReadingStatus(ReadingStatus status) => Create(status);
    }
}

