using HEVEQ.Domain.Enums;

namespace HEVEQ.Application.Features.Bookings.Helpers;

public static class BookingScheduleConflictHelper
{
    public static readonly BookingStatus[] BlockingStatuses =
    {
        BookingStatus.ConfirmedPendingPayment,
        BookingStatus.Active,
        BookingStatus.InProgress,
        BookingStatus.PendingCustomerConfirmation,
        BookingStatus.Completed,
        BookingStatus.Disputed,
        BookingStatus.PendingFieldVerification,
        BookingStatus.FieldVerificationComplete,
        BookingStatus.ResolvedReleased
    };

    public static readonly BookingStatus[] CustomerOpenBookingStatuses =
    {
        BookingStatus.PendingProviderResponse,
        BookingStatus.ConfirmedPendingPayment,
        BookingStatus.Active,
        BookingStatus.InProgress,
        BookingStatus.PendingCustomerConfirmation,
        BookingStatus.Disputed,
        BookingStatus.PendingFieldVerification,
        BookingStatus.FieldVerificationComplete
    };

    public static bool IsBlockingStatus(BookingStatus status)
    {
        return BlockingStatuses.Contains(status);
    }

    public static DateTime ToScheduledStart(DateOnly date, TimeOnly time)
    {
        return date.ToDateTime(time, DateTimeKind.Utc);
    }

    public static DateTime ToScheduledEnd(DateOnly date, TimeOnly time, decimal durationHours)
    {
        return ToScheduledStart(date, time).AddHours((double)durationHours);
    }

    public static bool Overlaps(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
    {
        return firstStart < secondEnd && firstEnd > secondStart;
    }
}
