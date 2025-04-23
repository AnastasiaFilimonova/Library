using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObject
{
    public class BookDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string GenreName { get; set; }
        public string Image { get; set; }
        public int PageCount { get; set; }
        public string Annotation { get; set; }
        public string ReadingStatusName { get; set; }
        public int? Rating { get; set; }
        public string Review { get; set; }
        public string Quotes { get; set; }
        public DateTime? StartReadingDate { get; set; }
        public DateTime? EndReadingDate { get; set; }
    }

}
