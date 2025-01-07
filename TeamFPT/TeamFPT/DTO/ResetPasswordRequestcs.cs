using System.ComponentModel.DataAnnotations;

namespace TeamFPT.DTO
{
    public class ResetPasswordRequestcs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }   
    }
}
