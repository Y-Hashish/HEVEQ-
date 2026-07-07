using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Reviews.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Reviews.Queries.GetMarketplaceSellerReviews;

public class GetMarketplaceSellerReviewsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMarketplaceSellerReviewsQuery, ReviewListDto>
{
    public async Task<ReviewListDto> Handle(GetMarketplaceSellerReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewedUserId == request.SellerId
                        && r.MarketplaceOrderId != null
                        && r.IsPublished)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ReviewerName = (r.Reviewer.FirstName + " " + r.Reviewer.LastName).Trim(),
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ReviewListDto
        {
            Items = reviews,
            TotalCount = reviews.Count,
            AverageRating = reviews.Count > 0 ? Math.Round((decimal)reviews.Average(r => r.Rating), 1) : 0m
        };
    }
}
