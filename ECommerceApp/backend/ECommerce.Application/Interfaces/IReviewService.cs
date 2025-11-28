using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Reviews;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetReviewsForProductAsync(int productId);
        Task<ReviewDto> AddReviewAsync(string userId, CreateReviewDto reviewDto);
        Task<bool> DeleteReviewAsync(int reviewId, string userId, bool isAdmin);
    }
}
