using System.ComponentModel.DataAnnotations;

namespace Project_Swagger.DTO
{
    public class UserDTO
    {
        [Required (ErrorMessage = "Must enter username")]
        [StringLength(50, ErrorMessage = "Invalid username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Must enter user")]
        [StringLength(50, ErrorMessage = "Invalid password")]
        public string PassWord { get; set; }
    }
}
