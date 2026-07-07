using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces
{
    public interface IReviewFilterService
    {
        Task<ReviewFilterResult> FilterReviewAsync(string reviewText, string serviceType, CancellationToken cancellationToken = default);
    }
}
