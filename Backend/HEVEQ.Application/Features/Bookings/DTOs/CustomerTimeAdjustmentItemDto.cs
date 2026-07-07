using HEVEQ.Domain.Enums;

namespace HEVEQ.Application.Features.Bookings.DTOs
{
    public sealed class CustomerTimeAdjustmentItemDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public decimal RequestedAdditionalHrs { get; set; }
        public decimal AdditionalCostAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusAr { get; set; } = string.Empty;
        public string? ProviderNote { get; set; }
        public DateTime? CustomerAcknowledgedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanPay { get; set; }
    }
}