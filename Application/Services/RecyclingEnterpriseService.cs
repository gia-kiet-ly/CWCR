using Application.Contract.DTOs;
using Application.Contract.Interfaces;
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

        // ================= CREATE =================
        public async Task<RecyclingEnterpriseDto> CreateAsync(
            CreateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = new RecyclingEnterprise
            {
                Name = dto.Name,
                Address = dto.Address,
                RepresentativeId = dto.RepresentativeId,
                Status = EnterpriseStatus.Pending
            };

            await repo.InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            return MapToDto(entity);
        }

        // ================= GET BY ID =================
        public async Task<RecyclingEnterpriseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.NoTrackingEntities
                .Include(x => x.Representative)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return entity == null ? null : MapToDto(entity);
        }

        // ================= GET ALL =================
        public async Task<IEnumerable<RecyclingEnterpriseDto>> GetAllAsync(
            RecyclingEnterpriseFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Representative)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            if (!string.IsNullOrEmpty(filter.Address))
                query = query.Where(x => x.Address.Contains(filter.Address));

            if (!string.IsNullOrEmpty(filter.Status) &&
                Enum.TryParse<EnterpriseStatus>(
                    filter.Status,
                    true,
                    out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var list = await query.ToListAsync();

            return list.Select(MapToDto);
        }

        // ================= UPDATE =================
        public async Task<bool> UpdateAsync(
            Guid id,
            UpdateRecyclingEnterpriseDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= UPDATE STATUS =================
        public async Task<bool> UpdateStatusAsync(
            Guid id,
            UpdateEnterpriseStatusDto dto)
        {
            var repo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var entity = await repo.GetByIdAsync(id);

            if (entity == null || entity.IsDeleted)
                return false;

            if (!Enum.TryParse<EnterpriseStatus>(
                dto.Status,
                true,
                out var newStatus))
                return false;

            entity.Status = newStatus;
            entity.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(entity);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= DELETE (SOFT) =================
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

        // ================= MAPPER =================
        private static RecyclingEnterpriseDto MapToDto(
            RecyclingEnterprise entity)
        {
            return new RecyclingEnterpriseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                RepresentativeId = entity.RepresentativeId,
                RepresentativeName = entity.Representative?.UserName,
                Status = entity.Status.ToString(),
                CreatedTime = entity.CreatedTime
            };
        }
    }
}