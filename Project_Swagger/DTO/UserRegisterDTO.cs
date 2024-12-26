namespace Project_Swagger.DTO
{
    public class UserRegisterDTO
    {
        public string UserName { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        public string Repassword { get; set; }
        public string Email { get; set; }
        public string Role { get; } = "user";
        public string OTP { get; } = Guid.NewGuid().ToString();
        public bool IsEmailVerified { get;} = false;
    }
}
