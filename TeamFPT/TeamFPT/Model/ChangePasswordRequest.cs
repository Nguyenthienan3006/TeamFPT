using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }  // Mật khẩu cũ

        [Required]
        public string NewPassword { get; set; }  // Mật khẩu mới
    }
}
