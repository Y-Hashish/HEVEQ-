using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class PaginatedFieldVerificationsResponse
    {
        public List<FieldVerificationDto> Items { get; set; } = new List<FieldVerificationDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
