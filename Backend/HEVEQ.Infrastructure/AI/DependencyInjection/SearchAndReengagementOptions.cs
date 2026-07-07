using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.AI.DependencyInjection;

public sealed class SearchAndReengagementOptions
{
    public required string OpenAiApiKey { get; init; }
}