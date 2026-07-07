using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Query.GetTicketDecisionContext
{
    public class GetTicketDecisionContextQueryHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
        : IRequestHandler<GetTicketDecisionContextQuery, TicketDecisionContextDto>
    {
        public async Task<TicketDecisionContextDto> Handle(GetTicketDecisionContextQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId ?? throw new ForbiddenAccessException("User is not authenticated.");
            var userRole = currentUserService.Role;

            if (userRole != "Admin" && userRole != "Employee")
            {
                throw new ForbiddenAccessException("You do not have access to this resource.");
            }

            var ticket = await context.Tickets
                .Include(t => t.Messages)
                    .ThenInclude(m => m.Attachments)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                throw new NotFoundException("Ticket", request.TicketId);
            }

            // Employee Access Rules: Can only view assigned tickets or unassigned tickets.
            if (userRole == "Employee")
            {
                if (ticket.AssignedToUserId.HasValue && ticket.AssignedToUserId.Value != userId)
                {
                    throw new ForbiddenAccessException("You do not have access to this ticket.");
                }
            }

            // Gather all user IDs to fetch names in one batch
            var userIds = new HashSet<Guid> { ticket.SubmittedById };
            if (ticket.AssignedToUserId.HasValue)
            {
                userIds.Add(ticket.AssignedToUserId.Value);
            }
            foreach (var m in ticket.Messages)
            {
                userIds.Add(m.SenderId);
            }

            var usersDict = await userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), cancellationToken);

            string statusAr = ticket.Status switch
            {
                TicketStatus.Open => "مفتوحة",
                TicketStatus.InProgress => "قيد المعالجة",
                TicketStatus.PendingCustomerReply => "بانتظار ردك",
                TicketStatus.PendingProviderReply => "بانتظار رد المزود",
                TicketStatus.PendingFieldVerification => "بانتظار التحقق الميداني",
                TicketStatus.Resolved => "محلولة",
                TicketStatus.Closed => "مغلقة",
                TicketStatus.Reopened => "معاد فتحها",
                _ => ticket.Status.ToString()
            };

            var result = new TicketDecisionContextDto
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                Subject = ticket.Subject,
                Status = ticket.Status.ToString(),
                StatusAr = statusAr,
                Category = ticket.Category.ToString(),
                Priority = GetPriorityString(ticket.Priority),
                CreatedAt = ticket.CreatedAt,
                AiSummary = ticket.AiSummary,
                AiIdentifiedIssue = ticket.AiIdentifiedIssue,
                AiClaimedImpact = ticket.AiClaimedImpact,
                AiEscalationPriority = ticket.AiEscalationPriority,
                SubmittedBy = new TicketSubmitterDto
                {
                    Id = ticket.SubmittedById,
                    DisplayName = usersDict.GetValueOrDefault(ticket.SubmittedById, "Unknown User")
                },
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToUserName = ticket.AssignedToUserId.HasValue ? usersDict.GetValueOrDefault(ticket.AssignedToUserId.Value, "Unknown") : null,
                Messages = ticket.Messages.OrderBy(m => m.CreatedAt).Select(m => new TicketMessageDto
                {
                    Id = m.Id,
                    SenderName = usersDict.GetValueOrDefault(m.SenderId, m.SenderId == ticket.SubmittedById ? "Customer" : "Admin/Employee"),
                    SenderId = m.SenderId,
                    SenderRole = m.SenderId == ticket.SubmittedById ? "User" : "Admin",
                    Body = m.Body,
                    Content = m.Body,
                    IsInternal = m.IsInternal,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };

            // Customer attachments: Gather all attachments uploaded by the ticket creator across all non-internal messages
            var customerMsgs = ticket.Messages.Where(m => m.SenderId == ticket.SubmittedById && !m.IsInternal);
            foreach (var m in customerMsgs)
            {
                foreach (var a in m.Attachments)
                {
                    result.CustomerAttachments.Add(new TicketAttachmentDto
                    {
                        Id = a.Id,
                        FileUrl = a.FileUrl,
                        FileName = a.FileName,
                        FileType = a.FileType.ToString(),
                        CreatedAt = a.CreatedAt
                    });
                }
            }

            // Booking Dispute details
            if (ticket.BookingId.HasValue)
            {
                var booking = await context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.EscrowRecords)
                    .Include(b => b.ServiceListing)
                        .ThenInclude(s => s.ProviderProfile)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(b => b.Id == ticket.BookingId.Value, cancellationToken);

                if (booking != null)
                {
                    result.LinkedBooking = new LinkedBookingDto
                    {
                        Id = booking.Id,
                        BookingReference = booking.BookingNumber ?? "N/A"
                    };

                    result.BookingDispute = new BookingDisputeDetailsDto
                    {
                        BookingId = booking.Id,
                        BookingNumber = booking.BookingNumber ?? "N/A",
                        TotalAmount = booking.EstimatedTotal,
                        ServiceName = booking.ServiceListing?.Title ?? "Service",
                        ProviderName = booking.ServiceListing?.ProviderProfile?.User != null 
                            ? $"{booking.ServiceListing.ProviderProfile.User.FirstName} {booking.ServiceListing.ProviderProfile.User.LastName}".Trim()
                            : "Provider",
                        CustomerName = booking.Customer != null 
                            ? $"{booking.Customer.FirstName} {booking.Customer.LastName}".Trim()
                            : "Customer",
                        DisputeOpenedAt = booking.DisputeOpenedAt,
                        DisputeReason = ticket.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault()?.Body ?? "Dispute reason not specified"
                    };

                    // Escrow Summary
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

                    // Provider Completion Evidence
                    var completionForm = await context.JobCompletionEvidenceForms
                        .Include(f => f.Photos)
                        .FirstOrDefaultAsync(f => f.BookingId == booking.Id, cancellationToken);

                    if (completionForm != null)
                    {
                        result.ProviderCompletionEvidence = new ProviderCompletionEvidenceDto
                        {
                            Id = completionForm.Id,
                            ProviderNotes = completionForm.ProviderNotes,
                            Status = completionForm.Status.ToString(),
                            SubmittedAt = completionForm.SubmittedAt,
                            Photos = completionForm.Photos.Select(p => new JobCompletionEvidencePhotoDto
                            {
                                Id = p.Id,
                                PhotoUrl = p.PhotoUrl,
                                Caption = p.Caption
                            }).ToList()
                        };
                    }

                    // Field Visits
                    var visits = await context.FieldVerificationForms
                        .Include(v => v.Photos)
                        .Include(v => v.DispatchedEmployee)
                        .Where(v => v.BookingId == booking.Id || (v.TicketId.HasValue && v.TicketId.Value == ticket.Id))
                        .ToListAsync(cancellationToken);

                    result.FieldVisits = visits.Select(v => new FieldVisitDetailsDto
                    {
                        Id = v.Id,
                        DispatchedEmployeeId = v.DispatchedEmployeeId,
                        DispatchedEmployeeName = v.DispatchedEmployee != null 
                            ? $"{v.DispatchedEmployee.FirstName} {v.DispatchedEmployee.LastName}".Trim()
                            : "Unknown Employee",
                        DispatchInstructions = v.DispatchInstructions,
                        VisitStatus = v.VisitStatus.ToString(),
                        VisitStatusAr = MapVisitStatusAr(v.VisitStatus),
                        EmployeeNotes = v.EmployeeNotes,
                        FieldVerificationOutcome = v.FieldVerificationOutcome?.ToString(),
                        AdminDecision = v.AdminDecision.ToString(),
                        AdminDecisionNote = v.AdminDecisionNote,
                        DispatchedAt = v.DispatchedAt,
                        VisitedAt = v.VisitedAt,
                        Photos = v.Photos.Select(p => new FieldVisitPhotoDto
                        {
                            Id = p.Id,
                            PhotoUrl = p.PhotoUrl,
                            Caption = p.Caption
                        }).ToList()
                    }).ToList();
                }
            }

            // Marketplace Order details
            if (ticket.MarketplaceOrderId.HasValue)
            {
                var order = await context.MarketplaceOrders
                    .Include(o => o.Buyer)
                    .Include(o => o.EscrowRecords)
                    .Include(o => o.Listing)
                        .ThenInclude(l => l.Seller)
                    .FirstOrDefaultAsync(o => o.Id == ticket.MarketplaceOrderId.Value, cancellationToken);

                if (order != null)
                {
                    result.LinkedMarketplaceOrder = new LinkedMarketplaceOrderDto
                    {
                        Id = order.Id,
                        OrderNumber = order.TrackingNumber ?? "N/A"
                    };

                    result.MarketplaceOrderDispute = new MarketplaceOrderDisputeDetailsDto
                    {
                        OrderId = order.Id,
                        OrderNumber = order.TrackingNumber ?? "N/A",
                        TotalAmount = order.Amount,
                        ListingTitle = order.Listing?.Title ?? "Product",
                        SellerName = order.Listing?.Seller != null 
                            ? $"{order.Listing.Seller.FirstName} {order.Listing.Seller.LastName}".Trim()
                            : "Seller",
                        BuyerName = order.Buyer != null 
                            ? $"{order.Buyer.FirstName} {order.Buyer.LastName}".Trim()
                            : "Buyer",
                        DisputeOpenedAt = order.CreatedAt, // order dispute date matches ticket opening date
                        DisputeReason = ticket.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault()?.Body ?? "Dispute reason not specified"
                    };

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

            // Available Dispute Decisions
            if (userRole == "Admin")
            {
                if (ticket.BookingId.HasValue)
                {
                    result.AvailableDisputeDecisions = new List<string> { "ReleaseToProvider", "RefundCustomer", "PartialSettlement", "SendFieldVerification", "EscalateToAdmin" };
                }
                else if (ticket.MarketplaceOrderId.HasValue)
                {
                    result.AvailableDisputeDecisions = new List<string> { "ReleaseToSeller", "RefundBuyer", "PartialSettlement", "EscalateToAdmin" };
                }
            }
            else
            {
                result.AvailableDisputeDecisions = new List<string>();
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

        private static string MapVisitStatusAr(VisitStatus status)
        {
            return status switch
            {
                VisitStatus.Dispatched => "تم الإرسال",
                VisitStatus.OnSite => "في الموقع",
                VisitStatus.Completed => "تم الانتهاء",
                VisitStatus.FailedAccess => "فشل الدخول",
                _ => status.ToString()
            };
        }
    }
}
