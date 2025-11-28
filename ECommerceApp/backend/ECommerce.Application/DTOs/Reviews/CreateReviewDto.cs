using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Comment { get; set; } = string.Empty;
    }
}
