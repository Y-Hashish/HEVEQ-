using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Identity;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HEVEQ.Application.Common.Exceptions;

namespace HEVEQ.Application.Features.Admin.Query.GetAdminTicketDetails
{
    public class GetAdminTicketDetailsQueryHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetAdminTicketDetailsQuery, AdminTicketDetailsDto>
    {
        public async Task<AdminTicketDetailsDto> Handle(GetAdminTicketDetailsQuery request, CancellationToken cancellationToken)
        {
            var ticketData = await context.Tickets
                .Where(t => t.Id == request.Id)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    t.Subject,
                    Status = t.Status.ToString(),
                    Category = t.Category.ToString(),
                    t.Priority,
                    t.SubmittedById, 
                    t.AssignedToUserId,
                    t.CreatedAt,

                    Messages = t.Messages.OrderBy(m => m.CreatedAt).Select(m => new
                    {
                        m.Id,
                        m.SenderId,
                        m.Body,
                        m.IsInternal,
                        m.CreatedAt
                    }).ToList(),

                    // الروابط (إذا كانت التذكرة مرتبطة بحجز أو طلب)
                    t.BookingId,
                    t.MarketplaceOrderId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (ticketData == null)
            {
                return null;
            }

            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Admin" && userRole != "Employee")
            {
                throw new ForbiddenAccessException("You do not have access to this resource.");
            }

            if (userRole == "Employee")
            {
                if (ticketData.AssignedToUserId.HasValue && ticketData.AssignedToUserId.Value != userId)
                {
                    throw new ForbiddenAccessException("You do not have access to this ticket.");
                }
            }

            // 2. تجميع معرّفات المستخدمين (User Ids) لجلب أسمائهم بكفاءة
            var userIdsToFetch = new HashSet<Guid> { ticketData.SubmittedById };
            if (ticketData.AssignedToUserId.HasValue)
            {
                userIdsToFetch.Add(ticketData.AssignedToUserId.Value);
            }
            foreach (var msg in ticketData.Messages)
            {
                userIdsToFetch.Add(msg.SenderId);
            }

            var usersDict = await userManager.Users
                .Where(u => userIdsToFetch.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);

            // 3. تعريب الحالة
            string statusAr = ticketData.Status switch
            {
                "Open" => "مفتوحة",
                "InProgress" => "قيد المعالجة",
                "Resolved" => "محلولة",
                "Closed" => "مغلقة",
                _ => ticketData.Status
            };

            // 4. تركيب الكائن النهائي (Mapping)
            var firstMessage = ticketData.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault();
            var result = new AdminTicketDetailsDto
            {
                Id = ticketData.Id,
                TicketNumber = ticketData.TicketNumber,
                Subject = ticketData.Subject,
                Title = ticketData.Subject, // Frontend compatibility
                Description = firstMessage?.Body ?? string.Empty,
                Status = ticketData.Status,
                StatusAr = statusAr,
                Priority = GetPriorityString(ticketData.Priority),
                Category = ticketData.Category,
                SubmittedByName = usersDict.GetValueOrDefault(ticketData.SubmittedById, "Unknown User"),
                UserName = usersDict.GetValueOrDefault(ticketData.SubmittedById, "Unknown User"), // Frontend compatibility
                CreatedAt = ticketData.CreatedAt,
                AssignedToUserId = ticketData.AssignedToUserId,
                AssignedToUserName = ticketData.AssignedToUserId.HasValue ? usersDict.GetValueOrDefault(ticketData.AssignedToUserId.Value, "Unknown") : null,

                SubmittedBy = new TicketSubmitterDto
                {
                    Id = ticketData.SubmittedById,
                    DisplayName = usersDict.GetValueOrDefault(ticketData.SubmittedById, "Unknown User")
                },

                Messages = ticketData.Messages.Select(m => new TicketMessageDto
                {
                    Id = m.Id,
                    SenderName = usersDict.GetValueOrDefault(m.SenderId, "System/Admin"),
                    SenderId = m.SenderId,
                    SenderRole = m.SenderId == ticketData.SubmittedById ? "User" : "Admin",
                    Body = m.Body,
                    Content = m.Body,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };

            if (userRole == "Admin")
            {
                if (ticketData.BookingId.HasValue)
                {
                    result.AvailableDisputeDecisions = new List<string> { "ReleaseToProvider", "RefundCustomer", "PartialSettlement", "SendFieldVerification", "EscalateToAdmin" };
                }
                else if (ticketData.MarketplaceOrderId.HasValue)
                {
                    result.AvailableDisputeDecisions = new List<string> { "ReleaseToSeller", "RefundBuyer", "PartialSettlement", "EscalateToAdmin" };
                }
            }
            else
            {
                result.AvailableDisputeDecisions = new List<string>(); // restrict all actions for Employees as we are unsure of business rules for them
            }

            if (ticketData.BookingId.HasValue)
            {
                var booking = await context.Bookings
                    .Include(b => b.EscrowRecords)
                    .FirstOrDefaultAsync(b => b.Id == ticketData.BookingId.Value, cancellationToken);
                if (booking != null)
                {
                    result.LinkedBooking = new LinkedBookingDto { Id = booking.Id, BookingReference = booking.BookingNumber ?? "N/A" };
                    var escrow = booking.EscrowRecords.FirstOrDefault(e => e.Status == EscrowStatus.Held || e.Status == EscrowStatus.Frozen) 
                                 ?? booking.EscrowRecords.FirstOrDefault();
                    if (escrow != null)
                    {
                        result.EscrowSummary = new EscrowSummaryDto
                        {
                            Id = escrow.Id,
                            GrossAmount = escrow.GrossAmount,
                            PlatformCommission = escrow.PlatformCommission,
                            ProviderPayout = escrow.ProviderPayout,
                            Status = escrow.Status.ToString()
                        };
                    }
                }
            }

            if (ticketData.MarketplaceOrderId.HasValue)
            {
                var order = await context.MarketplaceOrders
                    .Include(o => o.EscrowRecords)
                    .FirstOrDefaultAsync(o => o.Id == ticketData.MarketplaceOrderId.Value, cancellationToken);
                if (order != null)
                {
                    result.LinkedMarketplaceOrder = new LinkedMarketplaceOrderDto { Id = order.Id, OrderNumber = order.TrackingNumber };
                    var escrow = order.EscrowRecords.FirstOrDefault(e => e.Status == EscrowStatus.Held || e.Status == EscrowStatus.Frozen) 
                                 ?? order.EscrowRecords.FirstOrDefault();
                    if (escrow != null)
                    {
                        result.EscrowSummary = new EscrowSummaryDto
                        {
                            Id = escrow.Id,
                            GrossAmount = escrow.GrossAmount,
                            PlatformCommission = escrow.PlatformCommission,
                            ProviderPayout = escrow.ProviderPayout,
                            Status = escrow.Status.ToString()
                        };
                    }
                }
            }

            return result;
        }

        private static string GetPriorityString(int priority)
        {
            return priority switch
            {
                3 => "Urgent",
                2 => "High",
                1 => "Medium",
                0 => "Low",
                _ => "Medium"
            };
        }
    }
}
