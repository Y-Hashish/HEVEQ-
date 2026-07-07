using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces
{
    public interface IComplaintAnalysisService
    {
        Task<ComplaintAnalysisResult> AnalyzeComplaintAsync(string complaintText, CancellationToken cancellationToken = default);
    }
}
