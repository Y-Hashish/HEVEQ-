using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.Query.GetDashboardSummary
{
    public class GetDashboardSummaryQueryHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context)
        : IRequestHandler<GetDashboardSummaryQuery, AdminDashboardSummaryDTO>
    {
        public async Task<AdminDashboardSummaryDTO> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var response = new AdminDashboardSummaryDTO();

            // 1. إحصائيات المستخدمين (في استعلام واحد للأداء)
            var userStats = await userManager.Users
                .GroupBy(u => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(u => u.IsActive)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userStats != null)
            {
                response.TotalUsers = userStats.Total;
                response.ActiveUsers = userStats.Active;
            }

            // 2. إحصائيات مقدمي الخدمة
            var providers = await userManager.GetUsersInRoleAsync("Provider");
            response.TotalProviders = providers.Count;

            // 3. Sequential queries to prevent EF Core concurrent DbContext access exceptions
            response.PendingDocuments = await context.Documents
                .CountAsync(d => d.Status == DocumentVerificationStatus.Pending, cancellationToken);

            response.PendingServiceListings = await context.ServiceListings
                .CountAsync(s => s.Status == ServiceListingStatus.PendingReview, cancellationToken);

            response.PendingMarketplaceListings = await context.MarketplaceListings
                .CountAsync(m => m.Status == MarketplaceListingStatus.PendingReview, cancellationToken);

            response.ActiveBookings = await context.Bookings
                .CountAsync(b => b.Status == BookingStatus.InProgress, cancellationToken);

            response.DisputedBookings = await context.Bookings
                .CountAsync(b => b.DisputeOpenedAt.HasValue || b.Status == BookingStatus.Disputed, cancellationToken);

            response.DisputedMarketplaceOrders = await context.MarketplaceOrders
                .CountAsync(o => o.Status == MarketplaceOrderStatus.Disputed, cancellationToken);

            response.OpenTickets = await context.Tickets
                .CountAsync(t => t.Status == TicketStatus.Open ||
                                 t.Status == TicketStatus.PendingCustomerReply ||
                                 t.Status == TicketStatus.PendingProviderReply ||
                                 t.Status == TicketStatus.PendingFieldVerification, cancellationToken);

            response.FrozenEscrowRecords = await context.EscrowRecords
                .CountAsync(e => e.Status == EscrowStatus.Frozen, cancellationToken);
            response.EscrowFrozenCount = response.FrozenEscrowRecords;

            response.PendingFieldVerifications = await context.FieldVerificationForms
                .CountAsync(f => f.VisitStatus == VisitStatus.Dispatched ||
                                 f.VisitStatus == VisitStatus.OnSite ||
                                 (f.VisitStatus == VisitStatus.Completed && f.AdminDecision == FieldVerificationAdminDecision.Pending), cancellationToken);

            return response;
        }
    }
}
