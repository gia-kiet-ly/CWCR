using Domain.Base;
using Core.Enum;

namespace Domain.Entities
{
    public class EnterpriseDocument : BaseEntity
    {
        public Guid RecyclingEnterpriseId { get; set; }
        public RecyclingEnterprise RecyclingEnterprise { get; set; } = null!;

        public EnterpriseDocumentType DocumentType { get; set; }

        // Tên file gốc người dùng upload
        public string OriginalFileName { get; set; } = null!;

        // Tên file lưu trong hệ thống / blob / local
        public string StoredFileName { get; set; } = null!;

        // Đường dẫn truy cập file
        public string FileUrl { get; set; } = null!;

        public string? ContentType { get; set; }
        public long? FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }
}