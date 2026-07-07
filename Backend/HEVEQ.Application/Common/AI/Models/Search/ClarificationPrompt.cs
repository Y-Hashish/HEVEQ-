using HEVEQ.Application.Common.AI.Enums;
using System.Collections.Generic;


namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>
/// A clarifying question turn, formulated in the customer's detected language.
/// Returned by the handler when mandatory parameters are missing or when the
/// extracted location cannot be mapped to a supported Egyptian governorate.
/// </summary>
public sealed record ClarificationPrompt(
    IReadOnlyList<MissingSearchParameter> MissingParameters,
    string Message,
    ConversationLanguage Language);