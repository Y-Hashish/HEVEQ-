using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.AI.Models.Search;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Search.SearchServices.Queries
{
    /// <summary>
    /// Issued by the Angular UI on every search bar submission or Sticky Chat message.
    /// </summary>
    public sealed record SearchServicesQuery(
        string RawQuery,
        IReadOnlyList<ConversationTurn>? ConversationHistory,
        Guid? RequestingUserId,
        string? SessionId)
        : IRequest<SearchServicesResult>;
}