using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class Provinces
    {
        [Key]
        public int ProvinceID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProvinceName { get; set; }
        public virtual ICollection<Districts> District { get; set; }
    }
}
