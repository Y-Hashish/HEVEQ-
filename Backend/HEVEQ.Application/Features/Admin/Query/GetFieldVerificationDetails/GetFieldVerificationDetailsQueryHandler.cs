using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Query.GetFieldVerificationDetails
{
    public class GetFieldVerificationDetailsQueryHandler(IApplicationDbContext context) 
        : IRequestHandler<GetFieldVerificationDetailsQuery, FieldVerificationDetailsDto?>
    {
        public async Task<FieldVerificationDetailsDto?> Handle(GetFieldVerificationDetailsQuery request, CancellationToken cancellationToken)
        {
            // First, try to find a FieldVerificationForm by Id
            var form = await context.FieldVerificationForms
                .Include(f => f.Booking)
                    .ThenInclude(b => b.ServiceListing)
                        .ThenInclude(s => s.ProviderProfile)
                            .ThenInclude(p => p.User)
                .Include(f => f.DispatchedEmployee)
                .Include(f => f.Photos)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (form != null)
            {
                var locationDetails = $"{form.Booking.Governorate}, {form.Booking.District}, {form.Booking.Street}";

                return new FieldVerificationDetailsDto
                {
                    Id = form.Id,
                    ListingId = form.Booking.ServiceListingId,
                    ListingTitle = form.Booking.ServiceListing.Title,
                    ProviderName = form.Booking.ServiceListing.ProviderProfile.User.FirstName + " " + form.Booking.ServiceListing.ProviderProfile.User.LastName,
                    LocationDetails = locationDetails,
                    Status = form.VisitStatus == VisitStatus.Dispatched ? "Dispatched" :
                             form.VisitStatus == VisitStatus.OnSite ? "InProgress" :
                             form.VisitStatus == VisitStatus.Completed && form.AdminDecision == FieldVerificationAdminDecision.Pending ? "Completed" :
                             form.VisitStatus == VisitStatus.Completed && form.AdminDecision == FieldVerificationAdminDecision.ReleaseToProvider ? "Verified" : "Failed",
                    ScheduledDate = form.DispatchedAt,
                    EmployeeId = form.DispatchedEmployeeId,
                    EmployeeName = form.DispatchedEmployee.FirstName + " " + form.DispatchedEmployee.LastName,
                    CreatedAt = form.CreatedAt,
                    EmployeeReport = form.EmployeeNotes,
                    EvidenceUrls = form.Photos.Select(p => p.PhotoUrl).ToList()
                };
            }

            // If not found, try to find a Booking in Disputed status by Id (this represents a Pending dispatch)
            var booking = await context.Bookings
                .Include(b => b.ServiceListing)
                    .ThenInclude(s => s.ProviderProfile)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(b => b.Id == request.Id && b.Status == BookingStatus.Disputed, cancellationToken);

            if (booking != null)
            {
                var locationDetails = $"{booking.Governorate}, {booking.District}, {booking.Street}";

                return new FieldVerificationDetailsDto
                {
                    Id = booking.Id,
                    ListingId = booking.ServiceListingId,
                    ListingTitle = booking.ServiceListing.Title,
                    ProviderName = booking.ServiceListing.ProviderProfile.User.FirstName + " " + booking.ServiceListing.ProviderProfile.User.LastName,
                    LocationDetails = locationDetails,
                    Status = "Pending",
                    ScheduledDate = null,
                    EmployeeId = null,
                    EmployeeName = null,
                    CreatedAt = booking.CreatedAt,
                    EmployeeReport = null,
                    EvidenceUrls = new List<string>()
                };
            }

            return null;
        }
    }
}
