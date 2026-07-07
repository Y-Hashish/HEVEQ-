using HEVEQ.Application.Common.AI.Enums;
using System.Collections.Generic;

namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>
/// Structured search intent extracted from the customer's natural-language query.
/// EquipmentType and Location are intentionally nullable:
/// a first message often omits one or both, which is the exact condition that should
/// pause the search and return a clarifying question instead of a poorly-seeded
/// vector query.
/// </summary>
public sealed record SearchIntent(
    SearchTarget Target,
    string? EquipmentType,
    string? Location,
    string TaskDescription,
    ConversationLanguage Language)
{
    /// <summary>
    /// Returns the list of parameters that must be resolved before a vector search
    /// can proceed. Called by the handler to decide between a clarification turn and
    /// a search turn.
    /// </summary>
    public IReadOnlyList<MissingSearchParameter> GetMissingParameters()
    {
        var missing = new List<MissingSearchParameter>(2);
        if (string.IsNullOrWhiteSpace(EquipmentType)) missing.Add(MissingSearchParameter.EquipmentType);
        if (string.IsNullOrWhiteSpace(Location)) missing.Add(MissingSearchParameter.Location);
        return missing;
    }

    /// <summary>True when both EquipmentType and Location are present (geofencing
    /// validity is checked separately by IGeofenceValidator in the handler).</summary>
    public bool IsActionable => GetMissingParameters().Count == 0;
}