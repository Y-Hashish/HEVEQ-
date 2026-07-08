using System;

namespace HEVEQ.Application.Features.ServiceListings.DTOs
{
    public record PublicAvailabilityDto(
        Guid Id,
        int DayOfWeek,
        TimeOnly OpenTime,
        TimeOnly CloseTime
    );
}
