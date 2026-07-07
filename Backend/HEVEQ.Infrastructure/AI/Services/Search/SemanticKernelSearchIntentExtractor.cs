using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Enums;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Infrastructure.AI.Plugins;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Infrastructure.AI.Services.Search;

public sealed class SemanticKernelSearchIntentExtractor : ISearchIntentExtractor
{
    private readonly SearchServicesPlugin _plugin;

    public SemanticKernelSearchIntentExtractor(SearchServicesPlugin plugin)
        => _plugin = plugin;

    public async Task<SearchIntent> ExtractIntentAsync(
        string rawQuery,
        IReadOnlyList<ConversationTurn>? conversationHistory = null,
        CancellationToken ct = default)
    {
        var dto = await _plugin.ExtractIntentAsync(rawQuery, conversationHistory, ct);

        var language = dto.Language?.Equals("EgyptianArabic", StringComparison.OrdinalIgnoreCase) == true
            ? ConversationLanguage.EgyptianArabic
            : ConversationLanguage.English;

        return new SearchIntent(
            dto.EquipmentType,
            dto.Location,
            dto.TaskDescription ?? rawQuery,
            language);
    }
}