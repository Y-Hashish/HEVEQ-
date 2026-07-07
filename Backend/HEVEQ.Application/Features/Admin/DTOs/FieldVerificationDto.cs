using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class FieldVerificationDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
