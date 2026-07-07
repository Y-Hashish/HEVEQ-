using HEVEQ.Application.Features.Reviews.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Reviews.Queries.GetMarketplaceSellerReviews;

public record GetMarketplaceSellerReviewsQuery(Guid SellerId) : IRequest<ReviewListDto>;
