using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Options;
using Microsoft.Extensions.Options;
using AppImageUploadResult = HEVEQ.Application.Common.Models.ImageUploadResult;

namespace HEVEQ.Infrastructure.Services.Storage
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly CloudinarySettings _settings;
        private readonly Cloudinary _cloudinary;

        public ImageStorageService(IOptions<CloudinarySettings> cloudinaryOptions)
        {
            _settings = cloudinaryOptions.Value;

            ValidateSettings();

            var account = new Account(_settings.CloudName,_settings.ApiKey,_settings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<AppImageUploadResult> UploadImageAsync(Stream fileStream,string fileName,string contentType,string folder,
            CancellationToken cancellationToken = default)
        {
            if (fileStream is null)
                throw new InvalidOperationException("Image stream is required.");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new InvalidOperationException("File name is required.");

            if (string.IsNullOrWhiteSpace(folder))
                folder = "heveq/general";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error is not null)
            {
                throw new InvalidOperationException(
                    $"Cloudinary upload failed. Response: {uploadResult.Error.Message}");
            }

            var secureUrl = uploadResult.SecureUrl?.ToString();

            if (string.IsNullOrWhiteSpace(secureUrl))
                throw new InvalidOperationException("Cloudinary did not return a secure URL.");

            return new AppImageUploadResult
            {
                Url = secureUrl,
                PublicId = uploadResult.PublicId ?? string.Empty,
                Folder = folder,
                Format = uploadResult.Format ?? string.Empty,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Bytes = uploadResult.Bytes,
                IsPublic = true
            };
        }

        private void ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_settings.CloudName))
                throw new InvalidOperationException("Cloudinary:CloudName is missing.");

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException("Cloudinary:ApiKey is missing.");

            if (string.IsNullOrWhiteSpace(_settings.ApiSecret))
                throw new InvalidOperationException("Cloudinary:ApiSecret is missing.");
        }
    }
}