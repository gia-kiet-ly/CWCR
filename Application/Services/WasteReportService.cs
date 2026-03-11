using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class WasteReportService : IWasteReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRegionCodeResolver _regionCodeResolver;
        private readonly ICollectionRequestService _collectionRequestService;

        public WasteReportService(
            IUnitOfWork unitOfWork,
            IRegionCodeResolver regionCodeResolver,
            ICollectionRequestService collectionRequestService)
        {
            _unitOfWork = unitOfWork;
            _regionCodeResolver = regionCodeResolver;
            _collectionRequestService = collectionRequestService;
        }

        // ================= CREATE =================
        public async Task<WasteReportResponseDto> CreateAsync(
            CreateWasteReportDto dto,
            Guid citizenId)
        {
            if (!dto.Latitude.HasValue || !dto.Longitude.HasValue)
                throw new Exception("Location is required.");

            if (dto.Wastes == null || !dto.Wastes.Any())
                throw new Exception("At least one waste item is required.");

            var reportRepo = _unitOfWork.GetRepository<WasteReport>();
            var wasteItemRepo = _unitOfWork.GetRepository<WasteReportWaste>();
            var imageRepo = _unitOfWork.GetRepository<WasteImage>();

            string? regionCode = null;
            try
            {
                regionCode = await _regionCodeResolver.ResolveDistrictRegionCodeAsync(
                    dto.Latitude.Value,
                    dto.Longitude.Value);
            }
            catch
            {
                // MVP: nếu resolve fail thì fallback UNKNOWN
            }

            var report = new WasteReport
            {
                CitizenId = citizenId,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RegionCode = string.IsNullOrWhiteSpace(regionCode) ? "UNKNOWN" : regionCode,
                Status = WasteReportStatus.Pending
            };

            await reportRepo.InsertAsync(report);
            await _unitOfWork.SaveAsync();

            foreach (var wasteDto in dto.Wastes)
            {
                if (wasteDto.WasteTypeId == Guid.Empty)
                    throw new Exception("WasteTypeId is required.");

                if (wasteDto.Quantity <= 0)
                    throw new Exception("Quantity must be greater than 0.");

                var wasteItem = new WasteReportWaste
                {
                    WasteReportId = report.Id,
                    WasteTypeId = wasteDto.WasteTypeId,
                    Quantity = wasteDto.Quantity,
                    Note = wasteDto.Note
                };

                await wasteItemRepo.InsertAsync(wasteItem);
                await _unitOfWork.SaveAsync();

                if (wasteDto.Images != null && wasteDto.Images.Count > 0)
                {
                    foreach (var imageUrl in wasteDto.Images)
                    {
                        if (string.IsNullOrWhiteSpace(imageUrl))
                            continue;

                        await imageRepo.InsertAsync(new WasteImage
                        {
                            WasteReportWasteId = wasteItem.Id,
                            ImageUrl = imageUrl,
                            PublicId = ExtractPublicId(imageUrl)
                        });
                    }

                    await _unitOfWork.SaveAsync();
                }
            }

            await _collectionRequestService.CreateTop1RequestsForReportAsync(report.Id);

            return await GetByIdAsync(report.Id)
                   ?? throw new Exception("Create failed.");
        }
        // ================= REDISPATCH =================
        public async Task RedispatchAsync(Guid reportId)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(reportId);

            if (report == null || report.IsDeleted)
                throw new Exception("WasteReport not found.");

            if (report.Status != WasteReportStatus.NoEnterpriseAvailable)
                throw new Exception("Redispatch is not allowed.");

            report.Status = WasteReportStatus.Pending;
            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            await _collectionRequestService.CreateTop1RequestsForReportAsync(report.Id);
        }
        // ================= REJECT HISTORY =================
        public async Task<List<RejectHistoryDto>> GetRejectHistoryAsync(Guid reportId, Guid citizenId)
        {
            var reportRepo = _unitOfWork.GetRepository<WasteReport>();
            var requestRepo = _unitOfWork.GetRepository<CollectionRequest>();

            var report = await reportRepo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == reportId && x.CitizenId == citizenId && !x.IsDeleted);

            if (report == null)
                throw new Exception("Report not found or access denied.");

            var requests = await requestRepo.NoTrackingEntities
                .Include(x => x.Enterprise)
                .Include(x => x.WasteReportWaste)
                .Where(x =>
                    x.WasteReportWaste.WasteReportId == reportId &&
                    x.Status == CollectionRequestStatus.Rejected)
                .OrderByDescending(x => x.CreatedTime)
                .ToListAsync();

            return requests.Select(x => new RejectHistoryDto
            {
                RequestId = x.Id,
                EnterpriseId = x.EnterpriseId,
                EnterpriseName = x.Enterprise?.Name,
                RejectReason = x.RejectReason,
                RejectReasonName = x.RejectReason?.ToString(),
                RejectNote = x.RejectNote,
                CreatedTime = x.CreatedTime
            }).ToList();
        }
        // ================= UPDATE =================
        public async Task<WasteReportResponseDto> UpdateAsync(
        Guid id,
        UpdateWasteReportDto dto)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(id);

            if (report == null || report.IsDeleted)
                throw new Exception("WasteReport not found.");

            // ❌ Không cho sửa nếu đang dispute
            if (report.Status == WasteReportStatus.Disputed)
                throw new Exception("This report is under dispute and cannot be modified.");

            // ✅ Chỉ cho edit khi reject hoặc không có enterprise
            if (report.Status != WasteReportStatus.Rejected &&
                report.Status != WasteReportStatus.NoEnterpriseAvailable)
            {
                throw new Exception("This report cannot be modified.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
                report.Description = dto.Description;

            if (dto.Latitude.HasValue)
                report.Latitude = dto.Latitude;

            if (dto.Longitude.HasValue)
                report.Longitude = dto.Longitude;

            // reset status để dispatch lại
            report.Status = WasteReportStatus.Pending;

            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            // dispatch lại enterprise
            await _collectionRequestService.CreateTop1RequestsForReportAsync(report.Id);

            return await GetByIdAsync(id)
                   ?? throw new Exception("Update failed.");
        }

        // ================= GET BY ID =================
        public async Task<WasteReportResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.NoTrackingEntities
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.WasteType)
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return report == null ? null : MapToDto(report);
        }

        // ================= GET PAGED =================
        public async Task<PagedWasteReportDto> GetPagedAsync(WasteReportFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.WasteType)
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.Images)
                .Where(x => !x.IsDeleted);

            if (filter.CitizenId.HasValue)
                query = query.Where(x => x.CitizenId == filter.CitizenId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<WasteReportStatus>(filter.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(x =>
                    x.Description != null &&
                    x.Description.ToLower().Contains(keyword));
            }

            var paged = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedWasteReportDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(id);

            if (report == null || report.IsDeleted)
                return false;

            if (report.Status == WasteReportStatus.Collected ||
                report.Status == WasteReportStatus.Verified)
            {
                throw new Exception("This report cannot be deleted.");
            }

            report.IsDeleted = true;
            report.DeletedTime = DateTimeOffset.UtcNow;
            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<CitizenCollectionProofDto?> GetProofForCitizenAsync(Guid reportId, Guid citizenId)
        {
            var reportRepo = _unitOfWork.GetRepository<WasteReport>();
            var proofRepo = _unitOfWork.GetRepository<CollectionProof>();

            var report = await reportRepo.NoTrackingEntities
                .FirstOrDefaultAsync(r => r.Id == reportId && r.CitizenId == citizenId && !r.IsDeleted);

            if (report == null)
                throw new Exception("Report not found or access denied.");

            var proof = await proofRepo.NoTrackingEntities
                .Include(p => p.Assignment)
                    .ThenInclude(a => a.Request)
                        .ThenInclude(r => r.WasteReportWaste)
                .FirstOrDefaultAsync(p =>
                    p.Assignment.Request.WasteReportWaste.WasteReportId == reportId);

            if (proof == null)
                return null;

            return new CitizenCollectionProofDto
            {
                ProofId = proof.Id,
                CreatedTime = proof.CreatedTime,
                Notes = proof.Note,
                ReviewStatus = proof.ReviewStatus,
                Images = new List<string> { proof.ImageUrl }
            };
        }

        // ================= HELPERS =================
        private static string ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                var uploadIdx = Array.IndexOf(segments, "upload");

                if (uploadIdx < 0)
                    return url;

                var afterUpload = segments
                    .Skip(uploadIdx + 1)
                    .SkipWhile(s => s.Length > 1 && s[0] == 'v' && s.Skip(1).All(char.IsDigit))
                    .ToArray();

                var joined = string.Join("/", afterUpload);
                var dotIdx = joined.LastIndexOf('.');

                return dotIdx > 0 ? joined[..dotIdx] : joined;
            }
            catch
            {
                return url;
            }
        }

        // ================= MAPPER =================
        private static WasteReportResponseDto MapToDto(WasteReport report)
        {
            return new WasteReportResponseDto
            {
                Id = report.Id,
                CitizenId = report.CitizenId,
                Description = report.Description,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                Status = report.Status.ToString(),
                CreatedTime = report.CreatedTime,

                Wastes = report.Wastes.Select(x => new WasteItemResponseDto
                {
                    WasteTypeId = x.WasteTypeId,
                    WasteTypeName = x.WasteType?.Name,
                    Quantity = x.Quantity,
                    Note = x.Note,
                    ImageUrls = x.Images
                        .Where(i => !i.IsDeleted)
                        .Select(i => i.ImageUrl)
                        .ToList()
                }).ToList()
            };
        }
    }
}