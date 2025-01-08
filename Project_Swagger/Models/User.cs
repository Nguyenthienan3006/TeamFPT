namespace Project_Swagger.Models
{
    public class User
    {
        public int UserId { get; set; }      
        public string Fullname { get; set; }
        public string Email { get; set; }    
        public string Role { get; set; }
        public Account Account { get; set; }
    }
}
