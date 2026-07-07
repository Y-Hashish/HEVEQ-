using System;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace HEVEQ.Infrastructure.AI.Utilities;

internal static class StructuredSettings
{
    public static OpenAIPromptExecutionSettings JsonMode() =>
        new() { ResponseFormat = "json_object" };

    public static OpenAIPromptExecutionSettings JsonMode<T>() =>
        new() { ResponseFormat = typeof(T) };
}