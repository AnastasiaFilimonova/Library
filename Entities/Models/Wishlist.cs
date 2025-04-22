using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public int UserID { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(Book))]
        public int BookID { get; set; }
        public Book Book { get; set; }
    }
}
