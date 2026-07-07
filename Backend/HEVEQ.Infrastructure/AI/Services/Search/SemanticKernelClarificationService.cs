using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Enums;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Infrastructure.AI.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Infrastructure.AI.Services.Search;

public sealed class SemanticKernelClarificationService : IClarificationService
{
    private readonly SearchServicesPlugin _plugin;

    public SemanticKernelClarificationService(SearchServicesPlugin plugin)
        => _plugin = plugin;

    public async Task<ClarificationPrompt> BuildClarificationAsync(
        SearchIntent partialIntent,
        IReadOnlyList<MissingSearchParameter> missingParams,
        CancellationToken ct = default)
    {
        var missingStrings = missingParams
            .Select(p => p switch
            {
                MissingSearchParameter.EquipmentType => "EquipmentType",
                MissingSearchParameter.Location => "Location",
                MissingSearchParameter.InvalidLocation => "InvalidLocation",
                _ => p.ToString()
            })
            .ToList();

        var dto = await _plugin.GenerateClarificationAsync(
            partialIntent: new SearchIntentResponseDto(
                  partialIntent.Target.ToString(),
                partialIntent.EquipmentType,
                partialIntent.Location,
                partialIntent.TaskDescription,
                partialIntent.Language.ToString()),
            missingParams: missingStrings,
            ct: ct);

        var language = dto.Language?.Equals("EgyptianArabic", StringComparison.OrdinalIgnoreCase) == true
            ? ConversationLanguage.EgyptianArabic
            : ConversationLanguage.English;

        return new ClarificationPrompt(missingParams, dto.Message, language);
    }
}