using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IEnterpriseApprovalService
    {
        Task<PagedRecyclingEnterpriseDto> GetEnterprisesAsync(
            RecyclingEnterpriseFilterDto filter);

        Task<EnterpriseApprovalDetailDto?> GetEnterpriseApprovalDetailAsync(Guid enterpriseId);

        Task<EnterpriseApprovalResponseDto> ApproveAsync(
            ApproveEnterpriseRequestDto dto,
            Guid reviewedByUserId);

        Task<EnterpriseApprovalResponseDto> RejectAsync(
            RejectEnterpriseRequestDto dto,
            Guid reviewedByUserId);
    }
}
