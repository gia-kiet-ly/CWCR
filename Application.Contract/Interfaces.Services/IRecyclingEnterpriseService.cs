using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IRecyclingEnterpriseService
    {
        Task<EnterpriseProfileResponseDto> CreateOrUpdateProfileAsync(
            Guid userId,
            CreateOrUpdateEnterpriseProfileRequestDto dto);

        Task<EnterpriseProfileResponseDto?> GetMyEnterpriseProfileAsync(Guid userId);

        Task<EnterpriseDocumentResponseDto> UploadDocumentAsync(
            Guid userId,
            UploadEnterpriseDocumentRequestDto dto);

        Task<List<EnterpriseDocumentResponseDto>> GetMyDocumentsAsync(Guid userId);

        Task<EnterpriseProfileResponseDto> SetEnvironmentLicenseAsync(
            Guid userId,
            SetEnvironmentLicenseRequestDto dto);

        Task<SubmitEnterpriseProfileResponseDto> SubmitProfileAsync(
            Guid userId,
            SubmitEnterpriseProfileRequestDto dto);
    }
}