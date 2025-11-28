using AutoMapper;
using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsForProductAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsByProductIdAsync(productId);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto> AddReviewAsync(string userId, CreateReviewDto reviewDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(reviewDto.ProductId);
            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }

            var hasPurchased = await _unitOfWork.Reviews.HasUserPurchasedProductAsync(userId, reviewDto.ProductId);
            if (!hasPurchased)
            {
                throw new BadRequestException("You can only review products you have purchased and that have been delivered.");
            }
            
            var existingReview = await _unitOfWork.Reviews.FirstOrDefaultAsync(r => r.ProductId == reviewDto.ProductId && r.UserId == userId);
            if (existingReview != null)
            {
                throw new BadRequestException("You have already reviewed this product.");
            }

            var review = _mapper.Map<Review>(reviewDto);
            review.UserId = userId;
            review.IsVerifiedPurchase = true;

            await _unitOfWork.Reviews.AddAsync(review);
            
            // Mettre à jour la note moyenne du produit
            await UpdateProductRating(reviewDto.ProductId);
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("User {UserId} added a review for product {ProductId}", userId, reviewDto.ProductId);
            
            var reviewWithUser = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
            return _mapper.Map<ReviewDto>(reviewWithUser);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isAdmin)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new NotFoundException("Review not found.");
            }

            if (review.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            _unitOfWork.Reviews.Remove(review);

            // Mettre à jour la note moyenne du produit
            await UpdateProductRating(review.ProductId);

            var result = await _unitOfWork.SaveChangesAsync();
            
            if (result > 0)
            {
                _logger.LogInformation("Review {ReviewId} deleted by user {UserId}", reviewId, userId);
            }

            return result > 0;
        }
        
        private async Task UpdateProductRating(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product != null)
            {
                var reviews = await _unitOfWork.Reviews.FindAsync(r => r.ProductId == productId);
                if (reviews.Any())
                {
                    product.Rating = reviews.Average(r => r.Rating);
                    product.ReviewCount = reviews.Count();
                }
                else
                {
                    product.Rating = 0;
                    product.ReviewCount = 0;
                }
                _unitOfWork.Products.Update(product);
            }
        }
    }
}
