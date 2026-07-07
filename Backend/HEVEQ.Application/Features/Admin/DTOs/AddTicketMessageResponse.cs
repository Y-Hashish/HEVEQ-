using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.DTOs
{
    public class AddTicketMessageResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsInternal { get; set; }

        [JsonIgnore] public bool IsSuccess { get; set; }
        [JsonIgnore] public int StatusCode { get; set; }
    }
}
