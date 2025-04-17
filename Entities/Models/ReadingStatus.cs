using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class ReadingStatus
    {
        [Column("ReadingStatusID")]
        public int Id { get; set; }
        public int Status { get; set; }
        public int Rating { get; set; }
        public string Review { get; set; }
        public string Quotes { get; set; }
        public DateTime? StartReadingDate { get; set; }
        public DateTime? EndReadingDate { get; set; }
        public ICollection<Book> Books { get; set; }
    }
}
