using HEVEQ.Application.Common.Persistence.Models;
using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Persistence.Interfaces
{
    public interface IBookingReadRepository
    {
        // ── EXISTING - post-rejection re-engagement flow. Unchanged. ──────────────
        Task<BookingContext?> GetBookingContextAsync(Guid bookingId, CancellationToken ct = default);

        // ── NEW - pre-booking Concierge/Moderator evaluation. ──────────────────────
        Task<PreBookingContext?> GetPreBookingContextAsync(Guid bookingId, CancellationToken ct = default);
    }
}