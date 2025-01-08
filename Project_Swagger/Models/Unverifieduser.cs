using static System.Net.WebRequestMethods;

namespace Project_Swagger.Models
{
    public class UnverifiedUser
    {
        public int UnverifiedId { get; set; } 
        public int AccountId { get; set; }  
        public string Fullname { get; set; } 
        public string Role { get; set; } 
        public string Email { get; set; }
        public Account Account { get; set; }  
    }
}
