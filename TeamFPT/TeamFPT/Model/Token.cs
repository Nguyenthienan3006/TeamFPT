using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class Token
    {
        [Key]
        public int TokenId { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(50)]
        public string TokenType { get; set; }
        [Required]
        public string TokenValue { get; set; }
        public DateTime Expiration { get; set; }
        public virtual Users User { get; set; }
    }
}
