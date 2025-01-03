using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class UserAuthentication
    {
        [Key]
        public int UserId { get; set; }
        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        [Required, MaxLength(50)]
        public string Username { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string UserRole { get; set; } = "Customer";
        public virtual Users User { get; set; }
    }
}
