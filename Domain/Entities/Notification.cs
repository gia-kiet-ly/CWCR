using Domain.Base;
namespace Domain.Entities
{
    public class Notification : BaseEntity
    {
        // ================= RECEIVER =================
        public Guid UserId { get; set; }

        // ================= CONTENT =================
        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        // ================= TYPE =================
        public string Type { get; set; } = null!;

        // ================= STATUS =================
        public bool IsRead { get; set; } = false;

        public DateTimeOffset? ReadTime { get; set; }

        // ================= REFERENCE =================
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }
    }
}