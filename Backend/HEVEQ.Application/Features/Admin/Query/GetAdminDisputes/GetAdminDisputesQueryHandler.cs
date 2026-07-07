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

namespace HEVEQ.Application.Features.Admin.Query.GetAdminDisputes
{
    public class GetAdminDisputesQueryHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        : IRequestHandler<GetAdminDisputesQuery, PaginatedAdminDisputesResponse>
    {
        public async Task<PaginatedAdminDisputesResponse> Handle(GetAdminDisputesQuery request, CancellationToken cancellationToken)
        {
            // 1. Query Bookings
            var bookingsQuery = context.Bookings.AsQueryable();
            if (string.IsNullOrEmpty(request.Status))
            {
                bookingsQuery = bookingsQuery.Where(b => b.DisputeOpenedAt != null);
            }
            else if (request.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == BookingStatus.Disputed);
            }
            else if (request.Status.Equals("UnderReview", StringComparison.OrdinalIgnoreCase))
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == BookingStatus.PendingFieldVerification || b.Status == BookingStatus.FieldVerificationComplete);
            }
            else if (request.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                bookingsQuery = bookingsQuery.Where(b => b.DisputeOpenedAt != null && 
                                                        b.Status != BookingStatus.Disputed && 
                                                        b.Status != BookingStatus.PendingFieldVerification && 
                                                        b.Status != BookingStatus.FieldVerificationComplete);
            }
            else
            {
                bookingsQuery = bookingsQuery.Where(b => false);
            }

            var disputedBookings = await bookingsQuery
                .Select(b => new
                {
                    Id = b.Id,
                    Type = "Booking",
                    ReferenceNumber = b.BookingNumber, 
                    CustomerId = b.CustomerId,         
                    ProviderId = b.ServiceListing.ProviderProfileId,
                    Amount = b.EstimatedTotal, 
                    Status = b.Status == BookingStatus.Disputed ? "Disputed" :
                             (b.Status == BookingStatus.PendingFieldVerification || b.Status == BookingStatus.FieldVerificationComplete) ? "UnderReview" :
                             "Resolved",
                    CreatedAt = b.DisputeOpenedAt ?? b.CreatedAt
                })
                .ToListAsync(cancellationToken);

            // 2. Query Marketplace Orders
            var ordersQuery = context.MarketplaceOrders.AsQueryable();
            if (string.IsNullOrEmpty(request.Status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == MarketplaceOrderStatus.Disputed || context.Tickets.Any(t => t.MarketplaceOrderId == o.Id));
            }
            else if (request.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == MarketplaceOrderStatus.Disputed);
            }
            else if (request.Status.Equals("UnderReview", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = ordersQuery.Where(o => false); // Marketplace orders don't have UnderReview status
            }
            else if (request.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                ordersQuery = ordersQuery.Where(o => o.Status != MarketplaceOrderStatus.Disputed && context.Tickets.Any(t => t.MarketplaceOrderId == o.Id));
            }
            else
            {
                ordersQuery = ordersQuery.Where(o => false);
            }

            var disputedOrders = await ordersQuery
                .Select(o => new
                {
                    Id = o.Id,
                    Type = "MarketplaceOrder",
                    ReferenceNumber = o.OrderNumber ?? o.Id.ToString().Substring(0, 8).ToUpper(),
                    CustomerId = o.BuyerId,            
                    ProviderId = o.Listing.SellerId,   
                    Amount = o.Amount,                
                    Status = o.Status == MarketplaceOrderStatus.Disputed ? "Disputed" : "Resolved",
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var combinedDisputes = disputedBookings.Concat(disputedOrders)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();

            var totalCount = combinedDisputes.Count;

            var pagedData = combinedDisputes
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            if (!pagedData.Any())
            {
                return new PaginatedAdminDisputesResponse { TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
            }

            var userIds = pagedData.Select(d => d.CustomerId)
                .Concat(pagedData.Select(d => d.ProviderId))
                .Distinct()
                .ToList();

            var usersDict = await userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);

            var items = pagedData.Select(d => new AdminDisputeDto
            {
                Id = d.Id,
                Type = d.Type,
                ReferenceNumber = d.ReferenceNumber,
                CustomerName = usersDict.GetValueOrDefault(d.CustomerId, "Unknown Customer"),
                ProviderName = usersDict.GetValueOrDefault(d.ProviderId, "Unknown Provider"),
                Amount = d.Amount,
                Status = d.Status,
                StatusAr = d.Status == "Disputed" ? "متنازع عليه" : d.Status == "UnderReview" ? "قيد المراجعة" : d.Status == "Resolved" ? "تم الحل" : d.Status,
                CreatedAt = d.CreatedAt
            }).ToList();

            return new PaginatedAdminDisputesResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
