using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.AI.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.Search
{
    /// <summary>
    /// Parses a raw natural-language query (Arabic slang / English) plus the full
    /// conversation thread into a structured SearchIntent.
    /// Passing the full thread allows a one-word follow-up ("Giza", "رافعة") to be
    /// resolved against the assistant's most recent question without any server-side
    /// session state.
    /// Infrastructure implementation: SemanticKernelSearchIntentExtractor
    /// </summary>
    public interface ISearchIntentExtractor
    {
        Task<SearchIntent> ExtractIntentAsync(
            string rawQuery,
            IReadOnlyList<ConversationTurn>? conversationHistory = null,
            CancellationToken ct = default);
    }
}
