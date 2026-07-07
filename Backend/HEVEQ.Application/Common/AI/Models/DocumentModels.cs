using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Models
{

    #region Document Vision Extraction (OCR replacement)

    /// <summary>Maps onto Documents.ExtractedText (via ExtractedFields) / ConfidenceScore / KeyFieldsPresent / ExpiryDate / FailureReason.</summary>
    public record DocumentExtractionResult(
      Guid DocumentId,
      bool IsReadable,
      decimal ConfidenceScore,
      bool KeyFieldsPresent,
      DateOnly? ExpiryDate,
      IReadOnlyDictionary<string, string> ExtractedFields,
      string? FailureReason,
      string? AdminNote);

    #endregion
}
