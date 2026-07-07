using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Admin.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Application.Features.Admin.Command.DisputeDecision
{
    public class DisputeDecisionCommandHandler(
        IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        NotificationHelper notificationHelper)
        : IRequestHandler<DisputeDecisionCommand, TicketActionResponse>
    {
        public async Task<TicketActionResponse> Handle(DisputeDecisionCommand request, CancellationToken cancellationToken)
        {
            // Restrict all dispute decisions to Admin role
            if (request.AdminRole != "Admin")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 403,
                    Message = "Only Admins can submit dispute decisions."
                };
            }

            if (string.IsNullOrWhiteSpace(request.DecisionNote))
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Decision note is required."
                };
            }

            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

            if (ticket == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Ticket not found." };
            }

            // Ticket must be linked to either BookingId or MarketplaceOrderId, never both
            if (!ticket.BookingId.HasValue && !ticket.MarketplaceOrderId.HasValue)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Ticket is not linked to any booking or marketplace order dispute."
                };
            }

            if (ticket.BookingId.HasValue && ticket.MarketplaceOrderId.HasValue)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Ticket cannot be linked to both a booking and a marketplace order dispute."
                };
            }

            if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Ticket is already resolved or closed."
                };
            }

            // Setup transactional block using concrete DbContext if possible
            if (context is DbContext dbContext)
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await ProcessDecisionAsync(ticket, request, cancellationToken);
                    if (result.IsSuccess)
                    {
                        await context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                    return result;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {
                return await ProcessDecisionAsync(ticket, request, cancellationToken);
            }
        }

        private async Task<TicketActionResponse> ProcessDecisionAsync(
            Ticket ticket,
            DisputeDecisionCommand request,
            CancellationToken cancellationToken)
        {
            if (ticket.BookingId.HasValue)
            {
                return await ProcessBookingDisputeAsync(ticket, request, cancellationToken);
            }
            else
            {
                return await ProcessMarketplaceDisputeAsync(ticket, request, cancellationToken);
            }
        }

        private async Task<TicketActionResponse> ProcessBookingDisputeAsync(
            Ticket ticket,
            DisputeDecisionCommand request,
            CancellationToken cancellationToken)
        {
            var booking = await context.Bookings
                .Include(b => b.EscrowRecords)
                .Include(b => b.ServiceListing)
                    .ThenInclude(s => s.ProviderProfile)
                .FirstOrDefaultAsync(b => b.Id == ticket.BookingId.Value, cancellationToken);

            if (booking == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Linked booking not found." };
            }

            // Get active escrow
            var activeEscrow = booking.EscrowRecords
                .FirstOrDefault(e => e.Status == EscrowStatus.Held || e.Status == EscrowStatus.Frozen);

            if (activeEscrow == null && request.DecisionType != "EscalateToAdmin")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "No active (Held/Frozen) escrow record found for this booking."
                };
            }

            switch (request.DecisionType)
            {
                case "ReleaseToProvider":
                    booking.Status = BookingStatus.ResolvedReleased;
                    activeEscrow!.Status = EscrowStatus.Released;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.FullReleaseToProvider;

                    notificationHelper.DisputeReleased(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber ?? "N/A");
                    notificationHelper.DisputeResolved(booking.CustomerId, booking.Id, booking.BookingNumber ?? "N/A");
                    break;

                case "RefundCustomer":
                    booking.Status = BookingStatus.ResolvedRefunded;
                    activeEscrow!.Status = EscrowStatus.Refunded;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.FullRefundToCustomer;

                    notificationHelper.DisputeRefunded(booking.CustomerId, booking.Id, booking.BookingNumber ?? "N/A");
                    notificationHelper.DisputePartiallySettled(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber ?? "N/A");
                    break;

                case "PartialSettlement":
                    decimal totalSettlement = request.CustomerAmount + request.ProviderAmount;
                    if (totalSettlement > activeEscrow!.GrossAmount)
                    {
                        return new TicketActionResponse
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Total settlement amount ({totalSettlement}) exceeds the escrow gross amount ({activeEscrow.GrossAmount})."
                        };
                    }

                    booking.Status = BookingStatus.ResolvedReleased;
                    activeEscrow.Status = EscrowStatus.PartialSettled;
                    activeEscrow.PartialSettleCustomerAmt = request.CustomerAmount;
                    activeEscrow.PartialSettleProviderAmt = request.ProviderAmount;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.PartialSettlement;

                    notificationHelper.DisputePartiallySettled(booking.CustomerId, booking.Id, booking.BookingNumber ?? "N/A");
                    notificationHelper.DisputePartiallySettled(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber ?? "N/A");
                    break;

                case "SendFieldVerification":
                    if (!request.EmployeeId.HasValue)
                    {
                        return new TicketActionResponse
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = "Employee ID is required to dispatch field verification."
                        };
                    }

                    var employee = await userManager.Users
                        .Include(u => u.EmployeeProfile)
                        .FirstOrDefaultAsync(u => u.Id == request.EmployeeId.Value, cancellationToken);

                    if (employee == null)
                    {
                        return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Employee not found." };
                    }

                    var isEmployeeRole = await userManager.IsInRoleAsync(employee, "Employee");
                    if (!isEmployeeRole)
                    {
                        return new TicketActionResponse { IsSuccess = false, StatusCode = 400, Message = "Selected user does not have the Employee role." };
                    }

                    if (employee.EmployeeProfile == null || !employee.EmployeeProfile.IsAvailableForDispatch)
                    {
                        return new TicketActionResponse { IsSuccess = false, StatusCode = 400, Message = "Employee is currently not available for dispatch." };
                    }

                    var linkedEvidence = await context.JobCompletionEvidenceForms
                        .Where(e => e.BookingId == booking.Id)
                        .OrderByDescending(e => e.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (linkedEvidence == null)
                    {
                        return new TicketActionResponse { IsSuccess = false, StatusCode = 400, Message = "No completion evidence found to verify for this booking." };
                    }

                    var verificationForm = new FieldVerificationForm
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        TicketId = ticket.Id,
                        DispatchedEmployeeId = employee.Id,
                        DispatchedByAdminId = request.AdminId,
                        LinkedEvidenceFormId = linkedEvidence.Id,
                        DispatchInstructions = request.DecisionNote,
                        VisitStatus = VisitStatus.Dispatched,
                        DispatchedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.FieldVerificationForms.Add(verificationForm);

                    booking.Status = BookingStatus.PendingFieldVerification;
                    booking.FieldVerificationDispatchedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.PendingFieldVerification;
                    ticket.ResolutionType = TicketResolutionType.EscalatedToFieldVerification;

                    notificationHelper.FieldVerificationAssigned(employee.Id, verificationForm.Id, booking.BookingNumber ?? "N/A");
                    break;

                case "EscalateToAdmin":
                    ticket.EscalatedAt = DateTime.UtcNow;
                    ticket.EscalatedToAdminId = request.AdminId;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    break;

                default:
                    return new TicketActionResponse
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Unsupported decision type '{request.DecisionType}' for Booking disputes."
                    };
            }

            // Auto-update linked field verifications if they are pending admin decision
            var linkedVerifications = await context.FieldVerificationForms
                .Where(v => (v.BookingId == booking.Id || (v.TicketId.HasValue && v.TicketId.Value == ticket.Id)) 
                            && v.AdminDecision == FieldVerificationAdminDecision.Pending)
                .ToListAsync(cancellationToken);

            foreach (var verification in linkedVerifications)
            {
                verification.AdminDecision = request.DecisionType switch
                {
                    "ReleaseToProvider" => FieldVerificationAdminDecision.ReleaseToProvider,
                    "RefundCustomer" => FieldVerificationAdminDecision.RefundToCustomer,
                    "PartialSettlement" => FieldVerificationAdminDecision.PartialSettlement,
                    _ => verification.AdminDecision
                };
                verification.DecidedByAdminId = request.AdminId;
                verification.DecidedAt = DateTime.UtcNow;
                verification.AdminDecisionNote = request.DecisionNote;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            return new TicketActionResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                TicketId = ticket.Id,
                Status = ticket.Status.ToString(),
                Message = "Dispute decision submitted successfully."
            };
        }

        private async Task<TicketActionResponse> ProcessMarketplaceDisputeAsync(
            Ticket ticket,
            DisputeDecisionCommand request,
            CancellationToken cancellationToken)
        {
            var order = await context.MarketplaceOrders
                .Include(o => o.EscrowRecords)
                .Include(o => o.Listing)
                .FirstOrDefaultAsync(o => o.Id == ticket.MarketplaceOrderId.Value, cancellationToken);

            if (order == null)
            {
                return new TicketActionResponse { IsSuccess = false, StatusCode = 404, Message = "Linked marketplace order not found." };
            }

            var activeEscrow = order.EscrowRecords
                .FirstOrDefault(e => e.Status == EscrowStatus.Held || e.Status == EscrowStatus.Frozen);

            if (activeEscrow == null && request.DecisionType != "EscalateToAdmin")
            {
                return new TicketActionResponse
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "No active (Held/Frozen) escrow record found for this order."
                };
            }

            switch (request.DecisionType)
            {
                case "ReleaseToSeller":
                    order.Status = MarketplaceOrderStatus.Completed;
                    activeEscrow!.Status = EscrowStatus.Released;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.FullReleaseToProvider;

                    notificationHelper.DisputeReleased(order.Listing.SellerId, order.Id, order.OrderNumber ?? "N/A");
                    notificationHelper.DisputeResolved(order.BuyerId, order.Id, order.OrderNumber ?? "N/A");
                    break;

                case "RefundBuyer":
                    order.Status = MarketplaceOrderStatus.Completed;
                    activeEscrow!.Status = EscrowStatus.Refunded;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.FullRefundToCustomer;

                    notificationHelper.DisputeRefunded(order.BuyerId, order.Id, order.OrderNumber ?? "N/A");
                    notificationHelper.DisputeResolved(order.Listing.SellerId, order.Id, order.OrderNumber ?? "N/A");
                    break;

                case "PartialSettlement":
                    decimal totalSettlement = request.CustomerAmount + request.ProviderAmount;
                    if (totalSettlement > activeEscrow!.GrossAmount)
                    {
                        return new TicketActionResponse
                        {
                            IsSuccess = false,
                            StatusCode = 400,
                            Message = $"Total settlement amount ({totalSettlement}) exceeds the escrow gross amount ({activeEscrow.GrossAmount})."
                        };
                    }

                    order.Status = MarketplaceOrderStatus.Completed;
                    activeEscrow.Status = EscrowStatus.PartialSettled;
                    activeEscrow.PartialSettleCustomerAmt = request.CustomerAmount;
                    activeEscrow.PartialSettleProviderAmt = request.ProviderAmount;
                    activeEscrow.ReleasedAt = DateTime.UtcNow;

                    ticket.Status = TicketStatus.Resolved;
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedByUserId = request.AdminId;
                    ticket.AdminResolution = request.DecisionNote;
                    ticket.ResolutionType = TicketResolutionType.PartialSettlement;

                    notificationHelper.DisputePartiallySettled(order.BuyerId, order.Id, order.OrderNumber ?? "N/A");
                    notificationHelper.DisputePartiallySettled(order.Listing.SellerId, order.Id, order.OrderNumber ?? "N/A");
                    break;

                case "EscalateToAdmin":
                    ticket.EscalatedAt = DateTime.UtcNow;
                    ticket.EscalatedToAdminId = request.AdminId;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    break;

                default:
                    return new TicketActionResponse
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Message = $"Unsupported decision type '{request.DecisionType}' for Marketplace order disputes."
                    };
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            return new TicketActionResponse
            {
                IsSuccess = true,
                StatusCode = 200,
                TicketId = ticket.Id,
                Status = ticket.Status.ToString(),
                Message = "Dispute decision submitted successfully."
            };
        }
    }
}
