using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.PreBooking
{
    public interface IPreBookingOrchestrator
    {
        Task<PreBookingDecisionResult> EvaluateAsync(Guid bookingId, CancellationToken ct = default);
    }
}
