namespace Application.Contract.Interfaces.ExternalService
{
    public interface IImageService
    {
        /// <summary>
        /// Upload ảnh công khai lên Cloudinary
        /// </summary>
        Task<string> UploadImageAsync(Stream fileStream, string fileName);

        /// <summary>
        /// Xóa ảnh từ Cloudinary
        /// </summary>
        Task<bool> DeleteImageAsync(string publicId);
    }
}