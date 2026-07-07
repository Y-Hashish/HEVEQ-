using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class UserAdminDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailableForDispatch { get; set; }
        public decimal? TrustScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
