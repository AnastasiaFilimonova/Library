using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObject
{
    public class BookUpdateDTO
    {
        public int BookId { get; set; }

        public int? ReadingStatusID { get; set; }  // 1 - не прочитана, 2 - прочитана

        public int? Rating { get; set; }
        public string Review { get; set; }
        public string Quotes { get; set; }
        public DateTime? StartReadingDate { get; set; }
        public DateTime? EndReadingDate { get; set; }
    }
}
