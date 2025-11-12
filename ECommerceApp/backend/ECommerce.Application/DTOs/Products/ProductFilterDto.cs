namespace ECommerce.Application.DTOs.Products
{
    public class ProductFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? IsFeatured { get; set; }
        public string SortBy { get; set; } = "name";
        public bool Descending { get; set; }
    }
}
