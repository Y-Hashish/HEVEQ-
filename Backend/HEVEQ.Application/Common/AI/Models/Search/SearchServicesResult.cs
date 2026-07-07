using System.Collections.Generic;
using HEVEQ.Application.Common.AI.Enums;
using HEVEQ.Application.Common.AI.Models.Search;



namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>
/// Single serialisable response contract for both the Search Bar and Sticky Chat endpoints. 
/// One flat record with a Status discriminant.
/// </summary>
public sealed record SearchServicesResult(
    SearchResponseStatus Status,
    SearchIntent? Intent,
    IReadOnlyList<SearchResultItem>? Results,
    bool HasZeroResults,
    ClarificationPrompt? Clarification,
    long ProcessingMs)
{
    public static SearchServicesResult ReadyWithResults(
        SearchIntent intent, IReadOnlyList<SearchResultItem> results, long ms) =>
        new(SearchResponseStatus.ResultsReady, intent, results,
            HasZeroResults: results.Count == 0, Clarification: null, ProcessingMs: ms);

    public static SearchServicesResult NeedsClarification(
        SearchIntent partialIntent, ClarificationPrompt clarification, long ms) =>
        new(SearchResponseStatus.ClarificationNeeded, partialIntent,
            Results: null, HasZeroResults: false, clarification, ProcessingMs: ms);
}