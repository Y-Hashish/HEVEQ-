using AutoMapper;
using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Localization;
using HEVEQ.Application.Features.MarketPlaceOrders.Common;
using HEVEQ.Application.Features.MarketPlaceOrders.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Application.Common.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.MarketPlaceOrders.Commands.CompleteMarketplaceOrder
{
    public class CompleteMarketplaceOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, NotificationHelper notificationHelper) : IRequestHandler<CompleteMarketplaceOrderCommand, OrderActionResponse>
    {
        public async Task<OrderActionResponse> Handle(CompleteMarketplaceOrderCommand request, CancellationToken cancellationToken)
        {
            if (!currentUser.UserId.HasValue)
                throw new ForbiddenAccessException("User is not authenticated.");

            var order = await context.MarketplaceOrders
                 .Include(o => o.Listing)
                 .Include(o => o.EscrowRecords)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
                ?? throw new NotFoundException(nameof(MarketplaceOrder), request.OrderId);

            if (order.BuyerId != currentUser.UserId.Value)
                throw new ForbiddenAccessException("You are not authorized to complete this order.");

            if (order.Status != MarketplaceOrderStatus.Delivered)
                throw new ValidationException("Status",
                    $"Order cannot be completed from its current status '{order.Status}'. Expected '{MarketplaceOrderStatus.Delivered}'.");

            var now = DateTime.UtcNow;

            order.Status = MarketplaceOrderStatus.Completed;
            order.ConfirmedByBuyerAt = now;

            var activeEscrow = order.EscrowRecords
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault(e => e.Status == EscrowStatus.Held && e.FrozenAt == null);

            if (activeEscrow is null)
                throw new ValidationException("Escrow", "No active held escrow record was found for this marketplace order.");

            activeEscrow.Status = EscrowStatus.Released;
            activeEscrow.ReleasedAt = now;

            notificationHelper.MarketplaceOrderCompleted(order.Listing.SellerId,order.Id,order.OrderNumber);
            notificationHelper.EscrowReleased(order.Listing.SellerId, activeEscrow.Id, order.OrderNumber);


            await context.SaveChangesAsync(cancellationToken);

            return new OrderActionResponse(
                order.Id,
                order.Status.ToString(),
                order.Status.ToArabic(),
                "Order completed successfully and escrow released to seller earnings");

        }
    }
}
