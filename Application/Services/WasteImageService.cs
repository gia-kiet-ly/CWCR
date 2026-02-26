using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class WasteImageService : IWasteImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public WasteImageService(
            IUnitOfWork unitOfWork,
            IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        // ================= UPLOAD =================
        public async Task<WasteImageResponseDto> CreateAsync(CreateWasteImageDto dto)
        {
            var wasteReportWasteRepo = _unitOfWork.GetRepository<WasteReportWaste>();
            var wasteImageRepo = _unitOfWork.GetRepository<WasteImage>();

            var parent = await wasteReportWasteRepo.GetByIdAsync(dto.WasteReportWasteId);
            if (parent == null || parent.IsDeleted)
                throw new Exception("WasteReportWaste not found");

            // Upload to Cloudinary
            using var stream = dto.File.OpenReadStream();

            var uploadResult = await _imageService.UploadImageAsync(
                stream,
                dto.File.FileName
            );

            if (uploadResult == null)
                throw new Exception("Image upload failed");

            var entity = new WasteImage
            {
                WasteReportWasteId = dto.WasteReportWasteId,
                ImageUrl = uploadResult.Url,      
                PublicId = uploadResult.PublicId,
                ImageType = dto.ImageType
            };

            await wasteImageRepo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        // ================= GET BY ID =================
        public async Task<WasteImageResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteImage>();

            var entity = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        // ================= PAGING / FILTER =================
        public async Task<PagedWasteImageDto> GetPagedAsync(WasteImageFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<WasteImage>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            if (filter.WasteReportWasteId.HasValue)
                query = query.Where(x =>
                    x.WasteReportWasteId == filter.WasteReportWasteId.Value);

            if (filter.ImageType.HasValue)
                query = query.Where(x =>
                    x.ImageType == filter.ImageType.Value);

            var paged = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedWasteImageDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        // ================= DELETE (Cloud + DB) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteImage>();

            var entity = await repo.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted)
                return false;

            // Delete from Cloudinary first
            var deleteResult = await _imageService.DeleteImageAsync(entity.PublicId);

            if (!deleteResult)
                throw new Exception("Failed to delete image from cloud");

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= MAPPER =================
        private static WasteImageResponseDto MapToDto(WasteImage entity)
        {
            return new WasteImageResponseDto
            {
                Id = entity.Id,
                WasteReportWasteId = entity.WasteReportWasteId,
                ImageUrl = entity.ImageUrl,
                ImageType = entity.ImageType,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}