using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Author
    {
        [Column("AuthorID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Author name is a required field.")]
        public string AuthorName { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
