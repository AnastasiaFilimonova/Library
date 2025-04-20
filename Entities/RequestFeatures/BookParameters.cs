using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.RequestFeatures
{
    public class BookParameters 
    {
        public string? AuthorName { get; set; }
        public string? GenreName { get; set; }
        public int? Year { get; set; } // Год окончания чтения
        public int? Rating { get; set; }
        public int? Status { get; set; } // 1 = прочитана, 0 = не прочитана
    }
}
