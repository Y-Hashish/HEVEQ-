using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HEVEQ.Application.Features.MarketPlace.Commands.UpdateMarketplaceListing
{
    public class UpdateMarketplaceListingCommandHandler(IApplicationDbContext context,ICurrentUserService currentUser ,IBackgroundJobService backgroundJobService) : IRequestHandler<UpdateMarketplaceListingCommand>
    {
        public async Task Handle(UpdateMarketplaceListingCommand command, CancellationToken cancellationToken)
        {
            if (!currentUser.UserId.HasValue)
                throw new ForbiddenAccessException("User is not authenticated.");

            var request = command.Request;

            var listing = await context.MarketplaceListings
                .Include(l => l.Photos)
                .FirstOrDefaultAsync(l => l.Id == command.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(MarketplaceListing), command.Id);

            if (listing.SellerId != currentUser.UserId.Value)
                throw new ForbiddenAccessException("You are not authorized to update this listing.");


            var categoryExists = await context.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.Type == CategoryType.Marketplace, cancellationToken);

            if (!categoryExists)
                throw new NotFoundException(nameof(Category), request.CategoryId);

            // Check whether AI-sensitive fields have changed BEFORE updating the entity.
            var shouldReModerate =
                listing.Title != request.Title ||
                listing.Description != request.Description ||
                listing.Specifications != request.Specifications ||
                listing.Price != request.Price;

            listing.CategoryId = request.CategoryId;
            listing.Title = request.Title;
            listing.Condition = request.Condition;
            listing.YearOfManufacture = request.YearOfManufacture;
            listing.Description = request.Description;
            listing.Specifications = request.Specifications;
            listing.Price = request.Price;
            listing.IsNegotiable = request.IsNegotiable;
            listing.TransactionMethod = request.TransactionMethod;
            listing.Governorate = request.Governorate;
            listing.District = request.District;
            listing.VideoUrl = request.VideoUrl;

            var hasMinimumPhotos = listing.Photos.Count >= 3;
            if (listing.Status == MarketplaceListingStatus.Active)
            {
                listing.Status = MarketplaceListingStatus.PendingReview;
            }
            else
            {
                listing.Status = hasMinimumPhotos ? MarketplaceListingStatus.PendingReview : MarketplaceListingStatus.Draft;
            }

            listing.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            // Re-run AI moderation only when AI-relevant fields changed and the listing is ready for review.
            if (shouldReModerate && listing.Status == MarketplaceListingStatus.PendingReview)
            {
                backgroundJobService.EnqueueMarketplaceModeration(listing.Id);
            }
        }
    }
}
