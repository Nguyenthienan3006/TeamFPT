using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeamFPT.Model
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        [Required, MaxLength(50)]
        public string FirstName { get; set; }
        [Required, MaxLength(50)]
        public string LastName { get; set; }
        [Required, MaxLength(255)]
        public string Address { get; set; }
        [Required]
        [ForeignKey("Wards")]
        public int WardID { get; set; }

        public virtual Wards Ward { get; set; }

        public virtual ICollection<UserAuthentication> UserAuthentications { get; set; }
    }
}
