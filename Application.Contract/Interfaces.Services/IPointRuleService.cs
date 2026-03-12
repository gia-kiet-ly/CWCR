using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IPointRuleService
    {
        Task<PointRuleResponse> CreateAsync(CreatePointRuleRequest request);

        Task<PointRuleResponse> UpdateAsync(Guid wasteTypeId, UpdatePointRuleRequest request);

        Task<List<PointRuleResponse>> GetAllAsync();
    }
}
