namespace LoginProject.Models
{
    public class PasswordResetToken
    {
        public int TokenId { get; set; }        // Mã định danh token
        public int UserId { get; set; }         // Liên kết đến tài khoản
        public string ResetToken { get; set; }  // Token dùng để reset mật khẩu
        public DateTime ExpiresAt { get; set; } // Thời gian hết hạn của token
        public bool IsUsed { get; set; }        // Trạng thái token (mặc định: chưa sử dụng)

        public virtual User User { get; set; }
    }
}
