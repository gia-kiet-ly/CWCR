using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PointRuleService : IPointRuleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointRuleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PointRuleResponse> CreateAsync(CreatePointRuleRequest request)
        {
            var ruleRepo = _unitOfWork.GetRepository<PointRule>();
            var wasteTypeRepo = _unitOfWork.GetRepository<WasteType>();

            // Check WasteType tồn tại
            var wasteType = await wasteTypeRepo.GetByIdAsync(request.WasteTypeId);
            if (wasteType == null)
                throw new Exception("WasteType not found");

            // Check rule đã tồn tại chưa
            var existingRule = await ruleRepo.FirstOrDefaultAsync(x => x.WasteTypeId == request.WasteTypeId);
            if (existingRule != null)
                throw new Exception("Point rule for this WasteType already exists");

            var rule = new PointRule
            {
                WasteTypeId = request.WasteTypeId,
                BasePoint = request.BasePoint,
                IsActive = true
            };

            await ruleRepo.InsertAsync(rule);
            await _unitOfWork.SaveAsync();

            return new PointRuleResponse
            {
                Id = rule.Id,
                WasteTypeId = rule.WasteTypeId,
                BasePoint = rule.BasePoint,
                IsActive = rule.IsActive
            };
        }

        public async Task<PointRuleResponse> UpdateAsync(Guid wasteTypeId, UpdatePointRuleRequest request)
        {
            var ruleRepo = _unitOfWork.GetRepository<PointRule>();

            var rule = await ruleRepo.FirstOrDefaultAsync(x => x.WasteTypeId == wasteTypeId);

            if (rule == null)
                throw new Exception("Point rule not found");

            rule.BasePoint = request.BasePoint;
            rule.IsActive = request.IsActive;

            await ruleRepo.UpdateAsync(rule);
            await _unitOfWork.SaveAsync();

            return new PointRuleResponse
            {
                Id = rule.Id,
                WasteTypeId = rule.WasteTypeId,
                BasePoint = rule.BasePoint,
                IsActive = rule.IsActive
            };
        }

        public async Task<List<PointRuleResponse>> GetAllAsync()
        {
            var ruleRepo = _unitOfWork.GetRepository<PointRule>();

            var rules = await ruleRepo.GetAllAsync();

            return rules.Select(r => new PointRuleResponse
            {
                Id = r.Id,
                WasteTypeId = r.WasteTypeId,
                BasePoint = r.BasePoint,
                IsActive = r.IsActive
            }).ToList();
        }
    }
}
