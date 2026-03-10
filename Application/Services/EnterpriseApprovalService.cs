using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Base;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EnterpriseApprovalService : IEnterpriseApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnterpriseApprovalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedRecyclingEnterpriseDto> GetEnterprisesAsync(
            RecyclingEnterpriseFilterDto filter)
        {
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var query = enterpriseRepo.Entities
                .Include(x => x.User)
                .Include(x => x.Documents)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim();
                query = query.Where(x => x.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Address))
            {
                var address = filter.Address.Trim();
                query = query.Where(x => x.Address.Contains(address));
            }

            if (!string.IsNullOrWhiteSpace(filter.ApprovalStatus) &&
                Enum.TryParse<EnterpriseApprovalStatus>(filter.ApprovalStatus, true, out var approvalStatus))
            {
                query = query.Where(x => x.ApprovalStatus == approvalStatus);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.SubmittedAt ?? x.CreatedTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedRecyclingEnterpriseDto
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items.Select(MapEnterpriseProfileToDto).ToList()
            };
        }

        public async Task<EnterpriseApprovalDetailDto?> GetEnterpriseApprovalDetailAsync(Guid enterpriseId)
        {
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var enterprise = await enterpriseRepo.Entities
                .Include(x => x.User)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.Id == enterpriseId && !x.IsDeleted);

            if (enterprise == null)
            {
                return null;
            }

            return new EnterpriseApprovalDetailDto
            {
                Enterprise = MapEnterpriseProfileToDto(enterprise)
            };
        }

        public async Task<EnterpriseApprovalResponseDto> ApproveAsync(
            ApproveEnterpriseRequestDto dto,
            Guid reviewedByUserId)
        {
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.Id == dto.EnterpriseId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_not_found",
                    "Enterprise profile not found.");
            }

            enterprise.ApprovalStatus = EnterpriseApprovalStatus.Approved;
            enterprise.ReviewedAt = DateTime.UtcNow;
            enterprise.ReviewedByUserId = reviewedByUserId;
            enterprise.RejectionReason = null;
            enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;

            enterpriseRepo.Update(enterprise);
            await _unitOfWork.SaveAsync();

            return new EnterpriseApprovalResponseDto
            {
                EnterpriseId = enterprise.Id,
                ApprovalStatus = enterprise.ApprovalStatus.ToString(),
                ReviewedAt = enterprise.ReviewedAt,
                ReviewedByUserId = enterprise.ReviewedByUserId,
                RejectionReason = enterprise.RejectionReason,
                Message = "Enterprise approved successfully."
            };
        }

        public async Task<EnterpriseApprovalResponseDto> RejectAsync(
            RejectEnterpriseRequestDto dto,
            Guid reviewedByUserId)
        {
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.Id == dto.EnterpriseId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_not_found",
                    "Enterprise profile not found.");
            }

            enterprise.ApprovalStatus = EnterpriseApprovalStatus.Rejected;
            enterprise.ReviewedAt = DateTime.UtcNow;
            enterprise.ReviewedByUserId = reviewedByUserId;
            enterprise.RejectionReason = dto.Reason.Trim();
            enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;

            enterpriseRepo.Update(enterprise);
            await _unitOfWork.SaveAsync();

            return new EnterpriseApprovalResponseDto
            {
                EnterpriseId = enterprise.Id,
                ApprovalStatus = enterprise.ApprovalStatus.ToString(),
                ReviewedAt = enterprise.ReviewedAt,
                ReviewedByUserId = enterprise.ReviewedByUserId,
                RejectionReason = enterprise.RejectionReason,
                Message = "Enterprise rejected successfully."
            };
        }

        private static EnterpriseProfileResponseDto MapEnterpriseProfileToDto(RecyclingEnterprise entity)
        {
            return new EnterpriseProfileResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Name = entity.Name,
                TaxCode = entity.TaxCode,
                Address = entity.Address,
                LegalRepresentative = entity.LegalRepresentative,
                RepresentativePosition = entity.RepresentativePosition,
                EnvironmentLicenseFileId = entity.EnvironmentLicenseFileId,
                ApprovalStatus = entity.ApprovalStatus.ToString(),
                OperationalStatus = entity.OperationalStatus.ToString(),
                SubmittedAt = entity.SubmittedAt,
                ReviewedAt = entity.ReviewedAt,
                ReviewedByUserId = entity.ReviewedByUserId,
                RejectionReason = entity.RejectionReason,
                CreatedTime = entity.CreatedTime,
                Documents = entity.Documents?
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.UploadedAt)
                    .Select(MapEnterpriseDocumentToDto)
                    .ToList() ?? new List<EnterpriseDocumentResponseDto>()
            };
        }

        private static EnterpriseDocumentResponseDto MapEnterpriseDocumentToDto(EnterpriseDocument entity)
        {
            return new EnterpriseDocumentResponseDto
            {
                Id = entity.Id,
                RecyclingEnterpriseId = entity.RecyclingEnterpriseId,
                DocumentType = entity.DocumentType.ToString(),
                OriginalFileName = entity.OriginalFileName,
                StoredFileName = entity.StoredFileName,
                FileUrl = entity.FileUrl,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                UploadedAt = entity.UploadedAt,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}
