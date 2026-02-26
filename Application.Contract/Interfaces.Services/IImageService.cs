using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IImageService
    {
        Task<ImageUploadResultDto> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
    }
}