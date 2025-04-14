using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class User
    {
        [Column("UserID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Login is a required field.")]
        [MaxLength(50, ErrorMessage = "Maximum length for Login is 50 characters.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is a required field.")]
        public string Password { get; set; }

        [MaxLength(50, ErrorMessage = "Maximum length for UserName is 50 characters.")]
        public string UserName { get; set; }

        public ICollection<ListBook> ListBooks { get; set; }
        public ICollection<Wishlist> Wishlist { get; set; }
    }
}
