using HEVEQ.Application.Common.AI.Enums;
using HEVEQ.Application.Common.AI.Models.Search;

using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.Search
{
    /// <summary>
    /// Formulates a warm, localised clarifying question when SearchIntent
    /// is missing a required parameter or contains an unrecognised location.
    /// The question language (English / Egyptian Arabic) is detected automatically from
    /// the customer's input.
    /// Infrastructure implementation: SemanticKernelClarificationService
    /// </summary>
    public interface IClarificationService
    {
        Task<ClarificationPrompt> BuildClarificationAsync(
            SearchIntent partialIntent,
            IReadOnlyList<MissingSearchParameter> missingParams,
            CancellationToken ct = default);
    }
}
