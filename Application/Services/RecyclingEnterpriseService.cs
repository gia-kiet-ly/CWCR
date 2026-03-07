using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Base;
using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RecyclingEnterpriseService : IRecyclingEnterpriseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;

        public RecyclingEnterpriseService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
        }

        public async Task<EnterpriseProfileResponseDto> CreateOrUpdateProfileAsync(
            Guid userId,
            CreateOrUpdateEnterpriseProfileRequestDto dto)
        {
            var user = await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var duplicatedTaxCode = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId != userId &&
                !x.IsDeleted &&
                x.TaxCode == dto.TaxCode);

            if (duplicatedTaxCode != null)
            {
                throw new BaseException.BadRequestException(
                    "tax_code_already_exists",
                    "Tax code already exists.");
            }

            var enterprise = await enterpriseRepo.Entities
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                enterprise = new RecyclingEnterprise
                {
                    UserId = userId,
                    Name = dto.Name.Trim(),
                    TaxCode = dto.TaxCode.Trim(),
                    Address = dto.Address.Trim(),
                    LegalRepresentative = dto.LegalRepresentative.Trim(),
                    RepresentativePosition = dto.RepresentativePosition.Trim(),
                    EnvironmentLicenseFileId = dto.EnvironmentLicenseFileId,
                    ApprovalStatus = EnterpriseApprovalStatus.PendingApproval,
                    OperationalStatus = EnterpriseStatus.Active,
                    CreatedTime = DateTimeOffset.UtcNow
                };

                await enterpriseRepo.InsertAsync(enterprise);
            }
            else
            {
                //if (enterprise.ApprovalStatus == EnterpriseApprovalStatus.Approved)
                //{
                //    throw new BaseException.BadRequestException(
                //        "enterprise_already_approved",
                //        "Approved enterprise profile cannot be edited with this action.");
                //} nếu muốn 1 khi đã approve thì không cho update nữa

                enterprise.Name = dto.Name.Trim();
                enterprise.TaxCode = dto.TaxCode.Trim();
                enterprise.Address = dto.Address.Trim();
                enterprise.LegalRepresentative = dto.LegalRepresentative.Trim();
                enterprise.RepresentativePosition = dto.RepresentativePosition.Trim();
                enterprise.EnvironmentLicenseFileId = dto.EnvironmentLicenseFileId;

                // hễ profile thay đổi thì quay lại chờ duyệt
                enterprise.ApprovalStatus = EnterpriseApprovalStatus.PendingApproval;
                enterprise.SubmittedAt = DateTime.UtcNow;
                enterprise.ReviewedAt = null;
                enterprise.ReviewedByUserId = null;
                enterprise.RejectionReason = null;
                enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;

                enterpriseRepo.Update(enterprise);
            }

            await _unitOfWork.SaveAsync();

            enterprise = await enterpriseRepo.Entities
                .Include(x => x.Documents)
                .FirstAsync(x => x.UserId == userId && !x.IsDeleted);

            return MapEnterpriseProfileToDto(enterprise);
        }

        public async Task<EnterpriseProfileResponseDto?> GetMyEnterpriseProfileAsync(Guid userId)
        {
            await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();

            var enterprise = await enterpriseRepo.Entities
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                return null;
            }

            return MapEnterpriseProfileToDto(enterprise);
        }

        public async Task<EnterpriseDocumentResponseDto> UploadDocumentAsync(
            Guid userId,
            UploadEnterpriseDocumentRequestDto dto)
        {
            await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var documentRepo = _unitOfWork.GetRepository<EnterpriseDocument>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.BadRequestException(
                    "enterprise_profile_not_found",
                    "Please create enterprise profile before uploading documents.");
            }

            if (dto.File == null || dto.File.Length == 0)
            {
                throw new BaseException.BadRequestException(
                    "invalid_file",
                    "File is required.");
            }

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new BaseException.BadRequestException(
                    "unsupported_file_type",
                    "Only pdf, doc, docx, png, jpg, jpeg files are allowed.");
            }

            var saveResult = await _fileStorageService.SaveFileAsync(
                dto.File,
                "uploads/enterprise-documents");

            var document = new EnterpriseDocument
            {
                RecyclingEnterpriseId = enterprise.Id,
                DocumentType = dto.DocumentType,
                OriginalFileName = dto.File.FileName,
                StoredFileName = saveResult.StoredFileName,
                FileUrl = saveResult.FileUrl,
                ContentType = saveResult.ContentType,
                FileSize = saveResult.FileSize,
                UploadedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await documentRepo.InsertAsync(document);
            await _unitOfWork.SaveAsync();

            if (dto.DocumentType == EnterpriseDocumentType.EnvironmentLicense &&
                enterprise.EnvironmentLicenseFileId == null)
            {
                enterprise.EnvironmentLicenseFileId = document.Id;
                enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;
                enterpriseRepo.Update(enterprise);
                await _unitOfWork.SaveAsync();
            }

            return MapEnterpriseDocumentToDto(document);
        }

        public async Task<List<EnterpriseDocumentResponseDto>> GetMyDocumentsAsync(Guid userId)
        {
            await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var documentRepo = _unitOfWork.GetRepository<EnterpriseDocument>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_profile_not_found",
                    "Enterprise profile not found.");
            }

            var documents = await documentRepo.NoTrackingEntities
                .Where(x => x.RecyclingEnterpriseId == enterprise.Id && !x.IsDeleted)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();

            return documents.Select(MapEnterpriseDocumentToDto).ToList();
        }

        public async Task<EnterpriseProfileResponseDto> SetEnvironmentLicenseAsync(
            Guid userId,
            SetEnvironmentLicenseRequestDto dto)
        {
            await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var documentRepo = _unitOfWork.GetRepository<EnterpriseDocument>();

            var enterprise = await enterpriseRepo.Entities
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_profile_not_found",
                    "Enterprise profile not found.");
            }

            var document = await documentRepo.FirstOrDefaultAsync(x =>
                x.Id == dto.DocumentId &&
                x.RecyclingEnterpriseId == enterprise.Id &&
                !x.IsDeleted);

            if (document == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_document_not_found",
                    "Enterprise document not found.");
            }

            enterprise.EnvironmentLicenseFileId = document.Id;
            enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;

            enterpriseRepo.Update(enterprise);
            await _unitOfWork.SaveAsync();

            return MapEnterpriseProfileToDto(enterprise);
        }

        public async Task<SubmitEnterpriseProfileResponseDto> SubmitProfileAsync(
            Guid userId,
            SubmitEnterpriseProfileRequestDto dto)
        {
            await EnsureEnterpriseUserAsync(userId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var documentRepo = _unitOfWork.GetRepository<EnterpriseDocument>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId == userId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_profile_not_found",
                    "Enterprise profile not found.");
            }

            var hasAnyDocument = await documentRepo.NoTrackingEntities.AnyAsync(x =>
                x.RecyclingEnterpriseId == enterprise.Id && !x.IsDeleted);

            if (!hasAnyDocument)
            {
                throw new BaseException.BadRequestException(
                    "enterprise_documents_required",
                    "Please upload at least one enterprise document before submitting.");
            }

            if (enterprise.EnvironmentLicenseFileId == null)
            {
                throw new BaseException.BadRequestException(
                    "environment_license_required",
                    "Please select an environment license document before submitting.");
            }

            var selectedLicenseDocument = await documentRepo.FirstOrDefaultAsync(x =>
                x.Id == enterprise.EnvironmentLicenseFileId.Value &&
                x.RecyclingEnterpriseId == enterprise.Id &&
                !x.IsDeleted);

            if (selectedLicenseDocument == null)
            {
                throw new BaseException.BadRequestException(
                    "invalid_environment_license_document",
                    "Selected environment license document is invalid.");
            }

            enterprise.ApprovalStatus = EnterpriseApprovalStatus.PendingApproval;
            enterprise.SubmittedAt = DateTime.UtcNow;
            enterprise.ReviewedAt = null;
            enterprise.ReviewedByUserId = null;
            enterprise.RejectionReason = null;
            enterprise.LastUpdatedTime = DateTimeOffset.UtcNow;

            enterpriseRepo.Update(enterprise);
            await _unitOfWork.SaveAsync();

            return new SubmitEnterpriseProfileResponseDto
            {
                EnterpriseId = enterprise.Id,
                ApprovalStatus = enterprise.ApprovalStatus.ToString(),
                SubmittedAt = enterprise.SubmittedAt!.Value,
                Message = "Enterprise profile submitted successfully and is waiting for approval."
            };
        }

        private async Task<ApplicationUser> EnsureEnterpriseUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                throw new BaseException.NotFoundException(
                    "user_not_found",
                    "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role != SystemRoles.RecyclingEnterprise)
            {
                throw new BaseException.UnauthorizedException(
                    "enterprise_role_required",
                    "Only enterprise accounts can perform this action.");
            }

            if (!user.EmailConfirmed)
            {
                throw new BaseException.UnauthorizedException(
                    "email_not_confirmed",
                    "Please verify your email before continuing.");
            }

            if (!user.IsActive)
            {
                throw new BaseException.UnauthorizedException(
                    "account_inactive",
                    "Your account is inactive.");
            }

            return user;
        }

        private static EnterpriseProfileResponseDto MapEnterpriseProfileToDto(RecyclingEnterprise entity)
        {
            return new EnterpriseProfileResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Name = entity.Name,
                TaxCode = entity.TaxCode,
                Address = entity.Address,
                LegalRepresentative = entity.LegalRepresentative,
                RepresentativePosition = entity.RepresentativePosition,
                EnvironmentLicenseFileId = entity.EnvironmentLicenseFileId,
                ApprovalStatus = entity.ApprovalStatus.ToString(),
                OperationalStatus = entity.OperationalStatus.ToString(),
                SubmittedAt = entity.SubmittedAt,
                ReviewedAt = entity.ReviewedAt,
                ReviewedByUserId = entity.ReviewedByUserId,
                RejectionReason = entity.RejectionReason,
                CreatedTime = entity.CreatedTime,
                Documents = entity.Documents?
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.UploadedAt)
                    .Select(MapEnterpriseDocumentToDto)
                    .ToList() ?? new List<EnterpriseDocumentResponseDto>()
            };
        }

        private static EnterpriseDocumentResponseDto MapEnterpriseDocumentToDto(EnterpriseDocument entity)
        {
            return new EnterpriseDocumentResponseDto
            {
                Id = entity.Id,
                RecyclingEnterpriseId = entity.RecyclingEnterpriseId,
                DocumentType = entity.DocumentType.ToString(),
                OriginalFileName = entity.OriginalFileName,
                StoredFileName = entity.StoredFileName,
                FileUrl = entity.FileUrl,
                ContentType = entity.ContentType,
                FileSize = entity.FileSize,
                UploadedAt = entity.UploadedAt,
                IsDeleted = entity.IsDeleted
            };
        }
    }
}