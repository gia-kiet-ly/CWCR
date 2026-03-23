using Application.Contract.DTOs;
using Core.Enum;

namespace Application.Contract.Interfaces.Services
{
    public interface IImageService
    {
        Task<ImageUploadResultDto> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
        Task CheckSafeSearchAsync(Stream imageStream);
        Task<WasteCategory?> AnalyzeWasteCategoryAsync(Stream imageStream);
    }
}