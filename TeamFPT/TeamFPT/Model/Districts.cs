using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class Districts
    {
        [Key]
        public int DistrictID { get; set; }

        [ForeignKey("Provinces")]
        public int ProvinceID { get; set; }

        [Required, MaxLength(255)]
        public string DistrictName { get; set; }

        public virtual Provinces Province { get; set; }
        public virtual ICollection<Wards> Wards { get; set; }
    }
}
