namespace HEVEQ.Application.Common.AI.Models;

/// <summary>
/// Result of validating an extracted location string against the Egyptian governorate
/// list. When IsValid is true, NormalizedGovernorate holds the canonical governorate name 
/// (e.g. "القاهرة" / "Cairo") that replaces the raw extracted text in the intent before the 
/// vector search is executed.
/// </summary>
public sealed record GeofenceValidationResult(
    bool IsValid,
    string? NormalizedGovernorate,
    string? RawLocation);