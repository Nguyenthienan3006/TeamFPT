using System.ComponentModel.DataAnnotations;

namespace LoginProject.DTO
{
    public class PagingModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
        public int PageSize { get; set; }
    }
}
