using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Interfaces
{
    public interface IBackgroundJobService
    {
        void EnqueueMarketplaceModeration(Guid listingId);
        void EnqueueServiceModeration(Guid listingId);
        void EnqueueDocumentOcr(Guid docId);
    }
}
