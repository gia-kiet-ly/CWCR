using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IWasteTypeService
    {
        // ================= CREATE =================
        Task<WasteTypeResponseDto> CreateAsync(CreateWasteTypeDto dto);

        // ================= UPDATE =================
        Task<WasteTypeResponseDto> UpdateAsync(Guid id, UpdateWasteTypeDto dto);

        // ================= GET BY ID =================
        Task<WasteTypeResponseDto?> GetByIdAsync(Guid id);

        // ================= PAGING / FILTER =================
        Task<PagedWasteTypeDto> GetPagedAsync(WasteTypeFilterDto filter);

        // ================= DELETE (SOFT) =================
        Task<bool> DeleteAsync(Guid id);
    }
}
