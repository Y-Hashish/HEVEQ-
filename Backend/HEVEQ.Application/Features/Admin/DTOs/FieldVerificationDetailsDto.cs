using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class FieldVerificationDetailsDto : FieldVerificationDto
    {
        public string ListingTitle { get; set; } = string.Empty;
        public string LocationDetails { get; set; } = string.Empty;
        public string? EmployeeReport { get; set; }
        public List<string> EvidenceUrls { get; set; } = new List<string>();
    }
}
