using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HEVEQ.Domain.Enums;

namespace HEVEQ.Application.Features.Bookings.Queries.GetBookingEscrow
{
    public sealed class GetBookingEscrowQueryHandler : IRequestHandler<GetBookingEscrowQuery, BookingEscrowDto>
    {
        private readonly IApplicationDbContext _context;
        public GetBookingEscrowQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookingEscrowDto> Handle(GetBookingEscrowQuery request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .Include(x => x.EscrowRecords)
                .Include(x => x.TimeAdjustmentRequests)
                    .ThenInclude(x => x.EscrowRecords)
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            var isCustomer = string.Equals(request.Role, "Customer", StringComparison.OrdinalIgnoreCase);
            var isProvider = string.Equals(request.Role, "Provider", StringComparison.OrdinalIgnoreCase);
            var isAdmin = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            var isCustomerOwner = isCustomer && booking.CustomerId == request.UserId;

            var isProviderOwner =
                isProvider &&
                booking.ServiceListing != null &&
                booking.ServiceListing.ProviderProfile != null &&
                booking.ServiceListing.ProviderProfile.UserId == request.UserId;

            if (!isCustomerOwner && !isProviderOwner && !isAdmin)
                throw new UnauthorizedAccessException("You are not allowed to view escrow for this booking.");

            var relatedEscrows = booking.EscrowRecords
                .Concat(booking.TimeAdjustmentRequests.SelectMany(x => x.EscrowRecords))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (relatedEscrows.Count == 0)
            {
                return new BookingEscrowDto
                {
                    BookingId = booking.Id,
                    GrossAmount = 0,
                    PlatformCommission = 0,
                    ProviderPayout = 0,
                    VatAmount = 0,
                    Status = "NotCaptured",
                    StatusAr = "لم يتم التحصيل",
                    CapturedAt = null,
                    ReleasedAt = null,
                    FrozenAt = null
                };
            }

            var displayStatus = relatedEscrows.Any(x => x.Status == EscrowStatus.Frozen)
                ? EscrowStatus.Frozen
                : relatedEscrows.Any(x => x.Status == EscrowStatus.Held)
                    ? EscrowStatus.Held
                    : relatedEscrows.All(x => x.Status == EscrowStatus.Released)
                        ? EscrowStatus.Released
                        : relatedEscrows.First().Status;

            return new BookingEscrowDto
            {
                BookingId = booking.Id,
                GrossAmount = relatedEscrows.Sum(x => x.GrossAmount),
                PlatformCommission = relatedEscrows.Sum(x => x.PlatformCommission),
                ProviderPayout = relatedEscrows.Sum(x => x.ProviderPayout),
                VatAmount = relatedEscrows.Sum(x => x.VatAmount),
                Status = displayStatus.ToString(),
                StatusAr = EscrowDisplayHelper.GetStatusAr(displayStatus),
                CapturedAt = relatedEscrows.Where(x => x.CapturedAt.HasValue).Max(x => x.CapturedAt),
                ReleasedAt = relatedEscrows.Where(x => x.ReleasedAt.HasValue).Max(x => x.ReleasedAt),
                FrozenAt = relatedEscrows.Where(x => x.FrozenAt.HasValue).Max(x => x.FrozenAt)
            };
        }
    }
}