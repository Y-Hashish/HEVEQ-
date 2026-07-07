using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.Search
{
    /// <summary>
    /// Validates an extracted location string against Egypt's 27 supported governorates.
    /// Returns the canonical governorate name on success, or flags the location as
    /// invalid so the handler can request a targeted clarification instead of executing
    /// a poorly-bounded vector search.
    /// Infrastructure implementation: EgyptianGeofenceValidator
    /// </summary>
    public interface IGeofenceValidator
    {
        /// <summary>Synchronous — pure in-memory lookup against a static governorate set.</summary>
        GeofenceValidationResult Validate(string? rawLocation);
    }
}
