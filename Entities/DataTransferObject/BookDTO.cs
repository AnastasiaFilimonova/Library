using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObject
{
    public class BookDTO
    {
        public string Title { get; set; }
        public string AuthorName { get; set; } 
        public string GenreName { get; set; }
        public string Image { get; set; }
        public int PageCount { get; set; }
        public string Annotation { get; set; }
    }
}
