using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0.01, 1000000)]
        public decimal? Price { get; set; }

        [Range(0.01, 1000000)]
        public decimal? CompareAtPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }

        [StringLength(50)]
        public string? Sku { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsActive { get; set; }
    }
}
