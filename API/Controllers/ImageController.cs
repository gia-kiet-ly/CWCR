using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;

        public ImageController(IImageService imageService, IUnitOfWork unitOfWork)
        {
            _imageService = imageService;
            _unitOfWork = unitOfWork;
        }

        // =============================
        // Upload 1 ảnh
        // =============================
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { Message = "Invalid file format." });

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { Message = "File size exceeds 10MB." });

            try
            {
                using var stream = file.OpenReadStream();

                // [1] SafeSearch
                await _imageService.CheckSafeSearchAsync(stream);
                stream.Position = 0;

                // [2] Analyze waste category
                var suggestedCategory = await _imageService.AnalyzeWasteCategoryAsync(stream);
                stream.Position = 0;

                // [2.5] Map category -> WasteTypeId
                Guid? suggestedWasteTypeId = null;
                if (suggestedCategory.HasValue)
                {
                    var wasteTypeRepo = _unitOfWork.GetRepository<WasteType>();
                    var matched = await wasteTypeRepo.NoTrackingEntities
                        .Where(x => x.Category == suggestedCategory.Value && x.IsActive && !x.IsDeleted)
                        .FirstOrDefaultAsync();

                    suggestedWasteTypeId = matched?.Id;
                }

                // [3] Upload Cloudinary
                var result = await _imageService.UploadImageAsync(stream, file.FileName);

                return Ok(new
                {
                    result.Url,
                    result.PublicId,
                    SuggestedCategory = suggestedCategory?.ToString(),
                    SuggestedWasteTypeId = suggestedWasteTypeId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // =============================
        // Upload nhiều ảnh
        // =============================
        [HttpPost("upload-multiple")]
        [Authorize]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { Message = "No files uploaded." });

            if (files.Count > 5)
                return BadRequest(new { Message = "Maximum 5 images allowed." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            var uploaded = new List<object>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    errors.Add("Empty file skipped.");
                    continue;
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    errors.Add($"{file.FileName}: Invalid format.");
                    continue;
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    errors.Add($"{file.FileName}: Exceeds 10MB.");
                    continue;
                }

                try
                {
                    using var stream = file.OpenReadStream();

                    var result = await _imageService.UploadImageAsync(stream, file.FileName);

                    uploaded.Add(new
                    {
                        result.Url,
                        result.PublicId
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"{file.FileName}: {ex.Message}");
                }
            }

            return Ok(new
            {
                Uploaded = uploaded,
                Errors = errors,
                SuccessCount = uploaded.Count,
                FailureCount = errors.Count
            });
        }

        // =============================
        // Delete
        // =============================
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> Delete([FromQuery] string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return BadRequest(new { Message = "Public ID is required." });

            try
            {
                var success = await _imageService.DeleteImageAsync(publicId);

                if (!success)
                    return StatusCode(500, new { Message = "Failed to delete image." });

                return Ok(new { Message = "Image deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}