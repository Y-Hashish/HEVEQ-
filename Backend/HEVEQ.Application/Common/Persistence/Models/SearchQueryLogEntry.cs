using HEVEQ.Application.Common.Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Persistence.Models
{
    /// <summary>
    /// Maps directly onto a SearchQueryLogs INSERT.
    /// Both the ResultsReady and ClarificationNeeded paths write a row so the
    /// GAP-007 "zero-result rate" and "clarification rate" metrics are captured.
    /// </summary>
    public sealed record SearchQueryLogEntry(
        Guid? UserId,
        string? SessionId,
        string RawQuery,
        string? ExtractedIntentJson, // JSON of SearchIntent, or null on clarification
        SearchContextDomain ContextDomain,
        SearchMode SearchMode,
        int ResultCount,
        bool HasZeroResults,
        long ProcessingMs);
}
