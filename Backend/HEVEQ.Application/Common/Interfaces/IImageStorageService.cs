using HEVEQ.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Interfaces
{
    public interface IImageStorageService
    {
        Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, string contentType, string folder, CancellationToken cancellationToken = default);
    }
}
