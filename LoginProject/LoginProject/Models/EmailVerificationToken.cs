namespace LoginProject.Models
{
    public class EmailVerificationToken
    {
        public int TokenId { get; set; }           // Mã định danh token
        public int UserId { get; set; }            // Liên kết tới người dùng
        public string VerificationToken { get; set; } // Token xác thực email
        public DateTime ExpiresAt { get; set; }    // Thời gian hết hạn của token
        public bool IsUsed { get; set; }           // Trạng thái token đã sử dụng hay chưa (mặc định là chưa)

        public virtual User User { get; set; }
    }
}
