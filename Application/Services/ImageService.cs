using Application.Contract.DTOs;
using Application.Contract.Interfaces.ExternalService;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Application.ExternalService
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ImageUploadResultDto> UploadImageAsync(
            Stream fileStream,
            string fileName)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File stream is empty.");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "waste_images",
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidOperationException("Cloudinary upload failed.");

            if (uploadResult.Error != null)
                throw new InvalidOperationException(uploadResult.Error.Message);

            return new ImageUploadResultDto
            {
                Url = uploadResult.SecureUrl?.ToString()
                      ?? throw new InvalidOperationException("Upload returned null URL."),
                PublicId = uploadResult.PublicId
                           ?? throw new InvalidOperationException("Upload returned null PublicId.")
            };
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("PublicId cannot be null or empty.");

            var deletionParams = new DeletionParams(publicId);

            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }
    }
}