using HEVEQ.Application.Common.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Persistence.Interfaces
{
    public interface ISearchQueryLogWriter
    {
        /// <summary>Fire-and-forget safe — implementations should swallow non-critical exceptions
        /// to prevent analytics failures from surfacing to the customer.</summary>
        Task LogAsync(SearchQueryLogEntry entry, CancellationToken ct = default);
    }
}
