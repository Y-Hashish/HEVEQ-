using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Common.Persistence.Models;
using HEVEQ.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HEVEQ.Infrastructure.Persistence.Repositories;

/// <summary>
/// Writes one row to SearchQueryLogs per search turn (both ResultsReady and
/// ClarificationNeeded paths). All exceptions are caught and logged — analytics
/// writes must NEVER surface to the customer's search response.
/// </summary>
public sealed class SearchQueryLogWriter : ISearchQueryLogWriter
{
    // ADAPT: replace with your actual DbContext class name
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchQueryLogWriter> _logger;

    public SearchQueryLogWriter(
        ApplicationDbContext context,
        ILogger<SearchQueryLogWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(SearchQueryLogEntry entry, CancellationToken ct = default)
    {
        try
        {
            // Map to your SearchQueryLog EF entity
            // ADAPT: adjust property names to match your entity configuration
            var log = new SearchQueryLog
            {
                Id = Guid.NewGuid(),
                UserId = entry.UserId,
                SessionId = entry.SessionId,
                RawQuery = entry.RawQuery,
                ExtractedIntentJson = entry.ExtractedIntentJson,
                ContextDomain = (HEVEQ.Domain.Enums.SearchContextDomain)entry.ContextDomain,
                SearchMode = (HEVEQ.Domain.Enums.SearchMode)entry.SearchMode,
                ResultCount = entry.ResultCount,
                HasZeroResults = entry.HasZeroResults,
                HasLowConfidence = false,
                AlternativeSuggested = false,
                ProcessingMs = (int)Math.Min(entry.ProcessingMs, int.MaxValue),
                CreatedAt = DateTime.UtcNow
            };

            _context.SearchQueryLogs.Add(log);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Swallow — analytics failure must not break search
            _logger.LogWarning(ex, "SearchQueryLog write failed for query: {Query}",
                entry.RawQuery?[..Math.Min(50, entry.RawQuery?.Length ?? 0)]);
        }
    }
}