namespace Project_Swagger.Models
{
    public class User
    {
        public int UserId { get; set; }      
        public string Username { get; set; } 
        public string Password { get; set; } 
        public string Fullname { get; set; }
        public string Email { get; set; }    
        public string Role { get; set; }
        public string OTP { get; }
        public bool IsEmailVerified { get; set; }

    }
}
