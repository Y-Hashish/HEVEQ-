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

namespace HEVEQ.Application.Features.Admin.Query.GetFieldVerifications
{
    public class GetFieldVerificationsQueryHandler(IApplicationDbContext context) 
        : IRequestHandler<GetFieldVerificationsQuery, PaginatedFieldVerificationsResponse>
    {
        public async Task<PaginatedFieldVerificationsResponse> Handle(GetFieldVerificationsQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch pending dispatches (Bookings in Disputed status that don't have a FieldVerificationForm)
            var pendingBookingsQuery = context.Bookings
                .Include(b => b.ServiceListing)
                    .ThenInclude(s => s.ProviderProfile)
                        .ThenInclude(p => p.User)
                .Where(b => b.Status == BookingStatus.Disputed && !context.FieldVerificationForms.Any(f => f.BookingId == b.Id));

            var pendingItems = await pendingBookingsQuery
                .Select(b => new FieldVerificationDto
                {
                    Id = b.Id,
                    ListingId = b.ServiceListingId,
                    ProviderName = b.ServiceListing.ProviderProfile.User.FirstName + " " + b.ServiceListing.ProviderProfile.User.LastName,
                    Status = "Pending",
                    ScheduledDate = null,
                    EmployeeId = null,
                    EmployeeName = null,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync(cancellationToken);

            // 2. Fetch dispatched/completed verification forms
            var formsQuery = context.FieldVerificationForms
                .Include(f => f.Booking)
                    .ThenInclude(b => b.ServiceListing)
                        .ThenInclude(s => s.ProviderProfile)
                            .ThenInclude(p => p.User)
                .Include(f => f.DispatchedEmployee)
                .AsQueryable();

            var formItems = await formsQuery
                .Select(f => new FieldVerificationDto
                {
                    Id = f.Id,
                    ListingId = f.Booking.ServiceListingId,
                    ProviderName = f.Booking.ServiceListing.ProviderProfile.User.FirstName + " " + f.Booking.ServiceListing.ProviderProfile.User.LastName,
                    Status = f.VisitStatus == VisitStatus.Dispatched ? "Dispatched" :
                             f.VisitStatus == VisitStatus.OnSite ? "InProgress" :
                             f.VisitStatus == VisitStatus.Completed && f.AdminDecision == FieldVerificationAdminDecision.Pending ? "Completed" :
                             f.VisitStatus == VisitStatus.Completed && f.AdminDecision == FieldVerificationAdminDecision.ReleaseToProvider ? "Verified" : "Failed",
                    ScheduledDate = f.DispatchedAt,
                    EmployeeId = f.DispatchedEmployeeId,
                    EmployeeName = f.DispatchedEmployee.FirstName + " " + f.DispatchedEmployee.LastName,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync(cancellationToken);

            // 3. Combine both lists
            var combined = pendingItems.Concat(formItems)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // Apply status filter
            if (!string.IsNullOrEmpty(request.Status))
            {
                combined = combined.Where(x => x.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalCount = combined.Count;
            var paged = combined.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PaginatedFieldVerificationsResponse
            {
                Items = paged,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
