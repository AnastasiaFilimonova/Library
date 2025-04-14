using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Genre
    {
        [Column("GenreID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Genre name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the GenreName is 30 characters.")]
        public string GenreName { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
