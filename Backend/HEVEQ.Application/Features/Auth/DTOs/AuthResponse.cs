using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Auth.DTOs
{
    public class AuthResponse
    {
        public bool Succeeded { get; set; }
        public bool ProfileCompleted { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool RequiresEmailConfirmation { get; set; }
        public bool EmailConfirmationSent { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
