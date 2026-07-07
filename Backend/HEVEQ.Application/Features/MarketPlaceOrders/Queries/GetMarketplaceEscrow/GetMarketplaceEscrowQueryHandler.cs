using HEVEQ.Application.Common.Exceptions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Localization;
using HEVEQ.Application.Features.MarketPlaceOrders.DTOs;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.MarketPlaceOrders.Queries.GetMarketplaceEscrow
{
    public class GetMarketplaceEscrowQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) : IRequestHandler<GetMarketplaceEscrowQuery, MarketplaceEscrowDto>
    {
        public async Task<MarketplaceEscrowDto> Handle(GetMarketplaceEscrowQuery request, CancellationToken cancellationToken)
        {
            if (!currentUser.UserId.HasValue)
                throw new ForbiddenAccessException("User is not authenticated.");

            var order = await context.MarketplaceOrders
                 .AsNoTracking()
                 .Include(o => o.Listing)
                 .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
                 ?? throw new NotFoundException(nameof(MarketplaceOrder), request.OrderId);

            var userId = currentUser.UserId.Value;
            var isBuyer = order.BuyerId == userId;
            var isSeller = order.Listing.SellerId == userId;
            var isAdmin = string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);


            if (!isBuyer && !isSeller && !isAdmin)
                throw new ForbiddenAccessException("You are not allowed to view this escrow.");

            var escrow = await context.EscrowRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.MarketplaceOrderId == order.Id, cancellationToken);

            if (escrow is null)
            {
                return new MarketplaceEscrowDto
                {
                    OrderId = order.Id,
                    GrossAmount = order.Amount,
                    PlatformCommission = 0,
                    ProviderPayout = 0,
                    Status = order.Status == MarketplaceOrderStatus.PendingPayment ? "PendingPayment" : "NotCreated",
                    StatusAr = order.Status == MarketplaceOrderStatus.PendingPayment ? "في انتظار الدفع" : "لم يتم إنشاء سجل الضمان بعد",
                    CapturedAt = null,
                    ReleasedAt = null,
                    FrozenAt = null
                };
            }

            return new MarketplaceEscrowDto
            {
                OrderId = order.Id,
                GrossAmount = escrow.GrossAmount,
                PlatformCommission = escrow.PlatformCommission,
                ProviderPayout = escrow.ProviderPayout,
                Status = escrow.Status.ToString(),
                StatusAr = escrow.Status.ToArabic(),
                CapturedAt = escrow.CapturedAt,
                ReleasedAt = escrow.ReleasedAt,
                FrozenAt = escrow.FrozenAt
            };
        }
    }
}
