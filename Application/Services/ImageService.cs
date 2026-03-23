using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Core.Enum;
using Google.Cloud.Vision.V1;

namespace Application.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ImageAnnotatorClient _visionClient;

        public ImageService(Cloudinary cloudinary, ImageAnnotatorClient visionClient)
        {
            _cloudinary = cloudinary;
            _visionClient = visionClient;
        }

        // ================= UPLOAD =================
        public async Task<ImageUploadResultDto> UploadImageAsync(Stream fileStream, string fileName)
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

        // ================= DELETE =================
        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("PublicId cannot be null or empty.");

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok";
        }

        // ================= SAFE SEARCH =================
        public async Task CheckSafeSearchAsync(Stream imageStream)
        {
            var image = Image.FromStream(imageStream);
            var response = await _visionClient.DetectSafeSearchAsync(image);

            var likely = new[]
            {
                Likelihood.Likely,
                Likelihood.VeryLikely
            };

            if (likely.Contains(response.Adult))
                throw new InvalidOperationException("Ảnh chứa nội dung người lớn không phù hợp.");

            if (likely.Contains(response.Violence))
                throw new InvalidOperationException("Ảnh chứa nội dung bạo lực.");

            if (likely.Contains(response.Racy))
                throw new InvalidOperationException("Ảnh chứa nội dung nhạy cảm.");

            imageStream.Position = 0;
        }

        // ================= ANALYZE WASTE CATEGORY =================
        public async Task<WasteCategory?> AnalyzeWasteCategoryAsync(Stream imageStream)
        {
            var image = Image.FromStream(imageStream);
            var labels = await _visionClient.DetectLabelsAsync(image, maxResults: 10);

            if (labels == null || !labels.Any())
                return null;

            var labelNames = labels
                .Select(l => l.Description.ToLower())
                .ToHashSet();

            // Hazardous — ưu tiên check trước
            var hazardousKeywords = new[]
            {
                "battery", "chemical", "paint", "pesticide",
                "medicine", "syringe", "toxic", "hazardous",
                "electronic", "fluorescent", "bulb", "acid"
            };

            if (hazardousKeywords.Any(k => labelNames.Any(l => l.Contains(k))))
                return WasteCategory.Hazardous;

            // Recyclable
            var recyclableKeywords = new[]
            {
                "plastic", "bottle", "can", "metal", "glass",
                "paper", "cardboard", "carton", "aluminum",
                "newspaper", "magazine", "tin", "container"
            };

            if (recyclableKeywords.Any(k => labelNames.Any(l => l.Contains(k))))
                return WasteCategory.Recyclable;

            // Organic
            var organicKeywords = new[]
            {
                "food", "fruit", "vegetable", "leaf", "plant",
                "organic", "compost", "wood", "garden", "grass",
                "waste", "garbage", "trash", "rubbish", "litter"
            };

            if (organicKeywords.Any(k => labelNames.Any(l => l.Contains(k))))
                return WasteCategory.Organic;

            // Có liên quan đến rác nhưng không rõ loại
            var generalWasteKeywords = new[]
            {
                "waste", "garbage", "trash", "rubbish",
                "litter", "dump", "debris", "junk", "refuse"
            };

            if (generalWasteKeywords.Any(k => labelNames.Any(l => l.Contains(k))))
                return WasteCategory.Other;

            // Không phải ảnh rác
            return null;
        }
    }
}