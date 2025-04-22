using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObject
{
    public class WishlistDTO
    {
        public int BookID { get; set; }  // Переименуй, чтобы не путаться
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string GenreName { get; set; }
    }
}
