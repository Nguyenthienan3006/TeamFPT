namespace Project_Swagger.Models
{
    public class Account
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsVerify { get; set; }
    }
}
