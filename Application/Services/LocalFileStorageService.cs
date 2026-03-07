using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(string StoredFileName, string FileUrl, string? ContentType, long FileSize)> SaveFileAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("File is empty.");
            }

            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var normalizedFolder = folder.Replace("\\", "/").Trim('/');
            var targetFolder = Path.Combine(webRootPath, normalizedFolder);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(targetFolder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = "/" + $"{normalizedFolder}/{storedFileName}".Replace("\\", "/");

            return (
                StoredFileName: storedFileName,
                FileUrl: fileUrl,
                ContentType: file.ContentType,
                FileSize: file.Length
            );
        }
    }
}
