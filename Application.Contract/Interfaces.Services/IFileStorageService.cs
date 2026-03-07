using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<(string StoredFileName, string FileUrl, string? ContentType, long FileSize)> SaveFileAsync(
            IFormFile file,
            string folder);
    }
}
