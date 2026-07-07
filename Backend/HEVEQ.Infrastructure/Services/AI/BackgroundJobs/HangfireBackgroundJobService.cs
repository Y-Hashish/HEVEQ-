using Hangfire;
using HEVEQ.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.Services.AI.BackgroundJobs
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public void EnqueueDocumentOcr(Guid documentId)
      => BackgroundJob.Enqueue<DocumentOcrJob>(job => job.RunAsync(documentId));

        public void EnqueueMarketplaceModeration(Guid listingId)
        {
            BackgroundJob.Enqueue<MarketplaceModerationJob>(
            job => job.RunAsync(listingId));
        }
        public void EnqueueServiceModeration(Guid listingId)
        {
            BackgroundJob.Enqueue<ServiceListingModerationJob>(
                x => x.RunAsync(listingId, CancellationToken.None));
        }
    }
}
