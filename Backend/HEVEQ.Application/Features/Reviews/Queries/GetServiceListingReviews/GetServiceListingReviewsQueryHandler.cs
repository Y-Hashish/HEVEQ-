using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Reviews.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Reviews.Queries.GetServiceListingReviews;

public class GetServiceListingReviewsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetServiceListingReviewsQuery, ReviewListDto>
{
    public async Task<ReviewListDto> Handle(GetServiceListingReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.BookingId != null
                        && r.IsPublished
                        && (r.ServiceListingId == request.ServiceListingId
                            || (r.ServiceListingId == null
                                && r.Booking != null
                                && r.Booking.ServiceListingId == request.ServiceListingId)))
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ReviewerName = (r.Reviewer.FirstName + " " + r.Reviewer.LastName).Trim(),
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.PublishedAt ?? r.CreatedAt
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
