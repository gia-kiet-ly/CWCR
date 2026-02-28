using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
        public interface IRecyclingEnterpriseService
        {
            // ================================
            // CREATE
            // ================================
            Task<RecyclingEnterpriseDto> CreateAsync(CreateRecyclingEnterpriseDto dto);

            // ================================
            // READ
            // ================================
            Task<RecyclingEnterpriseDto?> GetByIdAsync(Guid id);

            Task<IEnumerable<RecyclingEnterpriseDto>> GetAllAsync(
                RecyclingEnterpriseFilterDto filter);

            // ================================
            // UPDATE
            // ================================
            Task<bool> UpdateAsync(Guid id, UpdateRecyclingEnterpriseDto dto);

            Task<bool> UpdateStatusAsync(Guid id, UpdateEnterpriseStatusDto dto);

            // ================================
            // DELETE
            // ================================
            Task<bool> DeleteAsync(Guid id);
        }
    }
