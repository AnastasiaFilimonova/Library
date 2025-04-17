using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Book
    {
        [Column("BookID")]
        public int Id { get; set; }

        public string? Title { get; set; }

        [ForeignKey(nameof(Author))]
        public int AuthorID { get; set; }
        public Author Author { get; set; }

        public string? Image { get; set; }

        [ForeignKey(nameof(ReadingStatus))]
        public int? ReadingStatusID { get; set; }
        public ReadingStatus? ReadingStatus { get; set; }


        [ForeignKey(nameof(Genre))]
        public int GenreID { get; set; }
        public Genre Genre { get; set; }

        public int PageCount { get; set; }

        public string? Annotation { get; set; }

        public ICollection<ListBook> ListBooks { get; set; }
        public ICollection<Wishlist> Wishlist { get; set; }
    }
}
