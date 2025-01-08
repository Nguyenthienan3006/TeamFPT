namespace Project_Swagger.Models
{
    public class Otp
    {
        public int OtpId { get; set; }  
        public int AccountId { get; set; }  
        public string OtpCode { get; set; }  
        public string TypeCode { get; set; }  
        public DateTime ExpirationTime { get; set; } 
        public bool IsUsed { get; set; } 
        public DateTime CreatedAt { get; set; } 
        public Account Account { get; set; } 
    }
}
