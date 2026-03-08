using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface ICollectorService
    {
        Task<CreateCollectorResponseDto> CreateCollectorAsync(
            Guid enterpriseUserId,
            CreateCollectorDto dto);

        Task<List<CollectorItemDto>> GetMyCollectorsAsync(
            Guid enterpriseUserId);
    }
}
