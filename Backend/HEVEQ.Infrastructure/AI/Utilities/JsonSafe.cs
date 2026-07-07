using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HEVEQ.Infrastructure.AI.Utilities
{
    internal static class JsonSafe
    {
        private static readonly JsonSerializerOptions Opts = new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static T DeserializeOrFallback<T>(string raw, T fallback)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(StripFences(raw), Opts) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static string StripFences(string raw)
        {
            var s = raw.Trim();
            if (s.StartsWith('{') || s.StartsWith('[')) return s;

            var objIdx = s.IndexOf('{');
            var arrIdx = s.IndexOf('[');
            int start = (objIdx, arrIdx) switch
            {
                ( >= 0, >= 0) => Math.Min(objIdx, arrIdx),
                ( >= 0, _) => objIdx,
                (_, >= 0) => arrIdx,
                _ => -1
            };
            if (start < 0) return s;

            s = s[start..];
            var lastBrace = s.LastIndexOf('}');
            var lastBracket = s.LastIndexOf(']');
            var end = Math.Max(lastBrace, lastBracket);
            if (end >= 0 && end < s.Length - 1) s = s[..(end + 1)];
            return s;
        }
    }
}
