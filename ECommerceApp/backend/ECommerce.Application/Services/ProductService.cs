using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ProductDto>> GetAllAsync(ProductFilterDto filter)
        {
            var query = await _unitOfWork.Products.GetAllAsync();

            // Recherche
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchLower = filter.Search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchLower) || 
                    p.Description.ToLower().Contains(searchLower));
            }

            // Filtres
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.InStock.HasValue)
                query = query.Where(p => p.StockQuantity > 0 == filter.InStock.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            query = query.Where(p => p.IsActive);

            // Tri
            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "rating" => filter.Descending ? query.OrderByDescending(p => p.Rating) : query.OrderBy(p => p.Rating),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => filter.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            var totalCount = query.Count();
            var items = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PagedResultDto<ProductDto>
            {
                Items = _mapper.Map<IEnumerable<ProductDto>>(items),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto?> GetBySlugAsync(string slug)
        {
            var product = await _unitOfWork.Products.GetBySlugAsync(slug);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Products.GetFeaturedProductsAsync(count);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            var slug = GenerateSlug(dto.Name);
            if (!await _unitOfWork.Products.IsSlugUniqueAsync(slug))
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

            var product = _mapper.Map<Product>(dto);
            product.Slug = slug;
            product.CreatedAt = DateTime.UtcNow;

            // Ajouter les images
            if (dto.ImageUrls.Any())
            {
                var images = dto.ImageUrls.Select((url, index) => new ProductImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0,
                    DisplayOrder = index,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                product.Images = images;
            }

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return null;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                product.Name = dto.Name;
                var newSlug = GenerateSlug(dto.Name);
                if (newSlug != product.Slug && await _unitOfWork.Products.IsSlugUniqueAsync(newSlug, id))
                    product.Slug = newSlug;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
                product.Description = dto.Description;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (dto.CompareAtPrice.HasValue)
                product.CompareAtPrice = dto.CompareAtPrice.Value;

            if (dto.StockQuantity.HasValue)
                product.StockQuantity = dto.StockQuantity.Value;

            if (!string.IsNullOrWhiteSpace(dto.Sku))
                product.Sku = dto.Sku;

            if (dto.CategoryId.HasValue)
                product.CategoryId = dto.CategoryId.Value;

            if (dto.IsFeatured.HasValue)
                product.IsFeatured = dto.IsFeatured.Value;

            if (dto.IsActive.HasValue)
                product.IsActive = dto.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                return false;

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}
