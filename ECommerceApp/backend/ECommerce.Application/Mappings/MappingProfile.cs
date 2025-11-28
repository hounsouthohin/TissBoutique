using AutoMapper;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.DTOs.Reviews;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Auth
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            // Products
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder)));

            CreateMap<ProductImage, ProductImageDto>();

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => 0));

            // Reviews
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<CreateReviewDto, Review>();

            // Cart
            CreateMap<Cart, CartDto>();

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSlug, opt => opt.MapFrom(src => src.Product.Slug))
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => 
                    src.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault() != null 
                        ? src.Product.Images.OrderBy(i => i.DisplayOrder).First().ImageUrl 
                        : null))
                .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.Product.StockQuantity > 0))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.Product.StockQuantity));

            // Orders
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => new ShippingAddressDto
                {
                    Street = src.ShippingStreet,
                    City = src.ShippingCity,
                    Province = src.ShippingProvince,
                    PostalCode = src.ShippingPostalCode,
                    Country = src.ShippingCountry
                }));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => 
                    src.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault() != null 
                        ? src.Product.Images.OrderBy(i => i.DisplayOrder).First().ImageUrl 
                        : null));

            CreateMap<Payment, PaymentInfoDto>()
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
