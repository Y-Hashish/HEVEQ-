using HEVEQ.Application.Features.Admin.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Features.Admin.Command.AddTicketMessage
{
    public class AddTicketMessageCommand : IRequest<AddTicketMessageResponse>
    {
        [JsonIgnore] public Guid TicketId { get; set; } 
        [JsonIgnore] public Guid AdminId { get; set; }  

        public string Body { get; set; } = string.Empty;
        public string Content { get => Body; set => Body = value; }
        public bool IsInternal { get; set; }
    }
}
