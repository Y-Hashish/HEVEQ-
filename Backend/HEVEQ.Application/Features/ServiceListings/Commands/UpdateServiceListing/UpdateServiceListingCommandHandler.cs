using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.ServiceListings.Commands.UpdateServiceListing;

public class UpdateServiceListingCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IBackgroundJobService backgroundJobService,
    NotificationHelper notificationHelper)
    : IRequestHandler<UpdateServiceListingCommand>
{
    public async Task Handle(UpdateServiceListingCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            throw new ForbiddenAccessException("User is not authenticated.");

        var userId = currentUserService.UserId.Value;

        var providerProfileId = await context.ProviderProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (providerProfileId == Guid.Empty)
            throw new ForbiddenAccessException("Only a registered provider can update a service listing.");

        var listing = await context.ServiceListings
            .SingleOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceListing), request.Id);

        if (listing.ProviderProfileId != providerProfileId)
            throw new ForbiddenAccessException("This listing does not belong to the current provider.");

        // Any provider edit must go through a new review cycle.
        // The edit is saved first as Draft, then the provider explicitly clicks
        // "إرسال للمراجعة" from the wizard. This mirrors marketplace UX while
        // keeping the AI/admin review as a separate final action.
        if (listing.Status is ServiceListingStatus.Active or ServiceListingStatus.PendingReview or ServiceListingStatus.Rejected)
        {
            listing.Status = ServiceListingStatus.Draft;
            listing.AdminRejectionNote = null;
            listing.AiRecommendation = null;
            listing.AiRiskFlags = null;
            listing.AiRiskLevel = null;
            listing.AiRiskScore = null;
        }

        listing.CategoryId = request.CategoryId;
        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Tags = request.Tags;
        listing.EquipmentModel = request.EquipmentModel;
        listing.EquipmentCapacity = request.EquipmentCapacity;
        listing.EquipmentCondition = request.EquipmentCondition;
        listing.YearOfManufacture = request.YearOfManufacture;
        listing.EquipmentRegistrationNumber = request.EquipmentRegistrationNumber;
        listing.HourlyRate = request.HourlyRate;
        listing.DailyRate = request.DailyRate;
        listing.MinimumBookingHours = request.MinimumBookingHours;
        listing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}
