using System.ComponentModel.DataAnnotations;

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
        public virtual ICollection<UserAuthentication> UserAuthentications { get; set; }
    }
}
