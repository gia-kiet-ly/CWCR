using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Base;
using Domain.Entities;

namespace Application.Services
{
    public class WasteReportService : IWasteReportService
    {
        private readonly IUnitOfWork _uow;

        public WasteReportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Guid> CreateAsync(CreateWasteReportDto dto)
        {
            if (dto.Latitude == null || dto.Longitude == null)
                throw new BaseException.BadRequestException(
                    "invalid_location",
                    "Latitude and Longitude are required."
                );

            var repo = _uow.GetRepository<WasteReport>();

            var entity = new WasteReport
            {
                Id = Guid.NewGuid(),
                CitizenId = dto.CitizenId,
                Description = dto.Description,
                Latitude = dto.Latitude.Value,
                Longitude = dto.Longitude.Value,
                Status = WasteReportStatus.Pending
            };

            await repo.InsertAsync(entity);
            await _uow.SaveAsync();

            return entity.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateWasteReportDto dto)
        {
            var repo = _uow.GetRepository<WasteReport>();

            var entity = await repo.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new BaseException.NotFoundException(
                    "report_not_found",
                    "Waste report not found."
                );

            entity.Description = dto.Description ?? entity.Description;

            if (dto.Latitude.HasValue)
                entity.Latitude = dto.Latitude.Value;

            if (dto.Longitude.HasValue)
                entity.Longitude = dto.Longitude.Value;

            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<WasteReportStatus>(dto.Status, true, out var status))
            {
                entity.Status = status;
            }

            repo.Update(entity);
            await _uow.SaveAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var repo = _uow.GetRepository<WasteReport>();

            var entity = await repo.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new BaseException.NotFoundException(
                    "report_not_found",
                    "Waste report not found."
                );

            repo.Delete(entity);
            await _uow.SaveAsync();
        }

        public async Task<WasteReportResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _uow.GetRepository<WasteReport>();

            var entity = await repo.FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return null;

            return new WasteReportResponseDto
            {
                Id = entity.Id,
                CitizenId = entity.CitizenId,
                Description = entity.Description,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Status = entity.Status.ToString()
            };
        }

        public async Task<PagedWasteReportDto> GetPagedAsync(WasteReportFilterDto filter)
        {
            var repo = _uow.GetRepository<WasteReport>();

            var list = await repo.GetAllAsync();
            var query = list.AsQueryable();

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

            var totalCount = query.Count();

            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WasteReportResponseDto
                {
                    Id = x.Id,
                    CitizenId = x.CitizenId,
                    Description = x.Description,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Status = x.Status.ToString()
                })
                .ToList();

            return new PagedWasteReportDto
            {
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = items
            };
        }
    }
}