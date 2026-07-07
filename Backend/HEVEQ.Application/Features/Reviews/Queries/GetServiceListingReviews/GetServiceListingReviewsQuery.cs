using HEVEQ.Application.Features.Reviews.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Reviews.Queries.GetServiceListingReviews;

public record GetServiceListingReviewsQuery(Guid ServiceListingId) : IRequest<ReviewListDto>;
