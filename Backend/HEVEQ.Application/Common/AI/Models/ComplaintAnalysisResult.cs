using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HEVEQ.Application.Common.AI.Models
{
    public class ComplaintAnalysisResult
    {
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "NORMAL";
    }
}
