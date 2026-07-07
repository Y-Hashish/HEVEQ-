namespace HEVEQ.Application.Features.Bookings.DTOs
{
    public class TimeAdjustmentPaymentConfirmResponseDto
    {
        public Guid TimeAdjustmentRequestId { get; set; }
        public Guid BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string TimeAdjustmentStatus { get; set; } = string.Empty;
        public string TimeAdjustmentStatusAr { get; set; } = string.Empty;
        public string EscrowStatus { get; set; } = string.Empty;
        public string EscrowStatusAr { get; set; } = string.Empty;
        public decimal BookingEstimatedDurationHours { get; set; }
        public decimal BookingEstimatedTotal { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}