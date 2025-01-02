using System.ComponentModel.DataAnnotations.Schema;

namespace LoginProject.Models
{
    public class User
    {
        public int UserId { get; set; }   
        public string Username { get; set; }  
        public string Email { get; set; }
        public string PasswordHash { get; set; }  
        public string Role { get; set; }    
        public bool IsEmailVerified { get; set; }  
        public DateTime RegisteredAt { get; set; }
    }

}
