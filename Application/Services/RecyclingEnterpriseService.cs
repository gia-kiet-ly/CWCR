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

        public async Task<RecyclingEnterpriseDto> CreateAsync(CreateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = new RecyclingEnterprise
            {
                UserId = dto.UserId,                       // ✅ bắt buộc
                Name = dto.Name,
                Address = dto.Address,
                RepresentativeId = dto.RepresentativeId,   // ✅ Guid?

                ApprovalStatus = EnterpriseApprovalStatus.PendingApproval,
                OperationalStatus = EnterpriseStatus.Active
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        public async Task<RecyclingEnterpriseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Representative)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<RecyclingEnterpriseDto>> GetAllAsync(RecyclingEnterpriseFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Representative)
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

            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var list = await query.ToListAsync();
            return list.Select(MapToDto);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted) return false;

            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.RepresentativeId = dto.RepresentativeId;

            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // DTO tên UpdateEnterpriseStatusDto giữ nguyên,
        // nhưng Status này được hiểu là ApprovalStatus
        public async Task<bool> UpdateStatusAsync(Guid id, UpdateEnterpriseStatusDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted) return false;

            if (!Enum.TryParse<EnterpriseApprovalStatus>(dto.Status, true, out var newStatus))
                return false;

            entity.ApprovalStatus = newStatus;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var entity = await repo.GetByIdAsync(id);

            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();
            return true;
        }

        private static RecyclingEnterpriseDto MapToDto(RecyclingEnterprise entity)
        {
            return new RecyclingEnterpriseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,

                // giữ field Status để tương thích
                Status = entity.ApprovalStatus.ToString(),

                RepresentativeId = entity.RepresentativeId,
                RepresentativeName = entity.Representative?.UserName,

                CreatedTime = entity.CreatedTime
            };
        }
    }
}