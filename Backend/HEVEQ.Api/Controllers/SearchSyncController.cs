
using HEVEQ.Infrastructure.AI.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace HEVEQ.Api.Controllers;

/// <summary>
/// Admin-only endpoints for managing the Qdrant vector index.
///
/// POST /api/admin/search/reindex
///   Pulls all Active ServiceListings from SQL Server, generates embeddings via
///   text-embedding-3-small, and upserts them into Qdrant with is_active=true.
///   Creates the collection automatically if it does not exist.
///   Safe to call repeatedly — subsequent calls overwrite existing points
///   (Qdrant upsert is idempotent on UUID point IDs).
///
/// GET  /api/admin/search/status
///   Returns the Qdrant collection name, existence flag, total vector count,
///   and collection health status — useful to verify the index before a demo.
///
/// Add [Authorize(Roles = "Admin")] once your JWT auth is wired,
/// or protect via an API-Gateway policy for the graduation demo.
/// </summary>
[ApiController]
[Route("api/admin/search")]
public sealed class SearchSyncController : ControllerBase
{
    private readonly QdrantSyncService _syncService;
    private readonly ILogger<SearchSyncController> _logger;

    public SearchSyncController(
        QdrantSyncService syncService,
        ILogger<SearchSyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    // ── POST /api/admin/search/reindex ────────────────────────────────────────

    /// <summary>
    /// Triggers a full reindex of all active service listings into Qdrant.
    ///
    /// Duration scales with listing count (~0.5 s per listing including the
    /// 300 ms inter-batch pause and the OpenAI embedding API round-trip).
    /// For 100 listings expect ~60 s; use the CancellationToken (request
    /// timeout) to abort if needed.
    ///
    /// Response shape:
    /// {
    ///   "totalListings": 87,
    ///   "synced": 85,
    ///   "failed": 2,
    ///   "failedIds": ["...", "..."],
    ///   "duration": "00:01:12.341"
    /// }
    /// </summary>
    [HttpPost("reindex")]
    [ProducesResponseType(typeof(ReindexResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReindexAsync(CancellationToken ct)
    {
        _logger.LogInformation("Qdrant reindex triggered via admin endpoint.");

        try
        {
            var result = await _syncService.SyncAllActiveListingsAsync(ct);

            return Ok(new ReindexResponse(
                result.TotalListings,
                result.Synced,
                result.Failed,
                result.FailedIds,
                result.Duration.ToString(@"hh\:mm\:ss\.fff")));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { message = "Reindex cancelled by client." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reindex failed with an unhandled exception.");
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // ── GET /api/admin/search/status ──────────────────────────────────────────

    /// <summary>
    /// Returns current Qdrant collection health and vector count.
    /// Run this after reindex to confirm all points were written.
    ///
    /// Response shape:
    /// {
    ///   "collectionName": "service_listings",
    ///   "exists": true,
    ///   "vectorCount": 85,
    ///   "statusText": "Green"
    /// }
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(StatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> StatusAsync(CancellationToken ct)
    {
        var status = await _syncService.GetCollectionStatusAsync(ct);
        return Ok(new StatusResponse(
            status.CollectionName,
            status.Exists,
            status.VectorCount,
            status.StatusText));
    }
}

// ─── Response records ─────────────────────────────────────────────────────────

public sealed record ReindexResponse(
    int TotalListings,
    int Synced,
    int Failed,
    IReadOnlyList<Guid> FailedIds,
    string Duration);

public sealed record StatusResponse(
    string CollectionName,
    bool Exists,
    ulong? VectorCount,
    string StatusText);