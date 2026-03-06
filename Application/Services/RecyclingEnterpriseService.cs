using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RecyclingEnterpriseService : IRecyclingEnterpriseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecyclingEnterpriseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CREATE
        public async Task<RecyclingEnterpriseDto> CreateAsync(Guid userId, CreateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = new RecyclingEnterprise
            {
                UserId = userId,

                Name = dto.Name,
                TaxCode = dto.TaxCode,
                Address = dto.Address,
                LegalRepresentative = dto.LegalRepresentative,
                RepresentativePosition = dto.RepresentativePosition,
                EnvironmentLicenseFileId = dto.EnvironmentLicenseFileId,

                ApprovalStatus = EnterpriseApprovalStatus.PendingApproval,
                OperationalStatus = EnterpriseStatus.Active,

                CreatedTime = DateTimeOffset.UtcNow
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        // GET BY ID
        public async Task<RecyclingEnterpriseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        // GET LIST + PAGING
        public async Task<PagedRecyclingEnterpriseDto> GetAllAsync(RecyclingEnterpriseFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Address))
                query = query.Where(x => x.Address.Contains(filter.Address));

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<EnterpriseApprovalStatus>(filter.Status, true, out var approval))
            {
                query = query.Where(x => x.ApprovalStatus == approval);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreatedTime)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedRecyclingEnterpriseDto
            {
                TotalCount = total,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items.Select(MapToDto).ToList()
            };
        }

        // UPDATE
        public async Task<bool> UpdateAsync(Guid id, UpdateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.Name = dto.Name;
            entity.TaxCode = dto.TaxCode;
            entity.Address = dto.Address;
            entity.LegalRepresentative = dto.LegalRepresentative;
            entity.RepresentativePosition = dto.RepresentativePosition;
            entity.EnvironmentLicenseFileId = dto.EnvironmentLicenseFileId;

            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // UPDATE APPROVAL STATUS (ADMIN)
        public async Task<bool> UpdateStatusAsync(Guid id, UpdateEnterpriseStatusDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            if (!Enum.TryParse<EnterpriseApprovalStatus>(dto.Status, true, out var newStatus))
                return false;

            entity.ApprovalStatus = newStatus;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // DELETE (SOFT DELETE)
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null)
                return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // MAPPING
        private static RecyclingEnterpriseDto MapToDto(RecyclingEnterprise entity)
        {
            return new RecyclingEnterpriseDto
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

                CreatedTime = entity.CreatedTime
            };
        }
    }
}