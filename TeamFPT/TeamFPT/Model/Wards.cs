using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TeamFPT.Model
{
    public class Wards
    {
        [Key]
        public int WardID { get; set; }

        [ForeignKey("Districts")]
        public int DistrictID { get; set; }

        [Required, MaxLength(255)]
        public string WardName { get; set; }

        public virtual Districts District { get; set; }
    }
}
