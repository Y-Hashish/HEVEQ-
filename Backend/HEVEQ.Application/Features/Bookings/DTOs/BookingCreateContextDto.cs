using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Bookings.DTOs
{
    public class BookingCreateContextDto
    {
        public Guid ServiceListingId { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;
        public string ProviderCompany { get; set; } = string.Empty;
        public decimal? HourlyRate { get; set; }
        public decimal? DailyRate { get; set; }
        public int MinimumBookingHours { get; set; }
        public decimal? ProviderBaseLatitude { get; set; }
        public decimal? ProviderBaseLongitude { get; set; }
        public decimal? ServiceRadiusKm { get; set; }
        public decimal OutOfZoneSurchargePerKm { get; set; } = 25m;
        public IReadOnlyList<BookingCreateContextAvailabilityDto> Availability { get; set; }
            = new List<BookingCreateContextAvailabilityDto>();
        public IReadOnlyList<BookingUnavailableSlotDto> UnavailableSlots { get; set; }
            = new List<BookingUnavailableSlotDto>();
        public BookingCreateContextAddressDto? DefaultAddress { get; set; }
        public BookingCustomerEligibilityDto CustomerEligibility { get; set; } = new();
    }
}
