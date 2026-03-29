using Domain.Base;

namespace Domain.Entities
{
    public class EnterpriseWasteCapability : BaseEntity
    {
        public Guid EnterpriseId { get; set; }
        public RecyclingEnterprise Enterprise { get; set; } = null!;

        public Guid WasteTypeId { get; set; }
        public WasteType WasteType { get; set; } = null!;

        public decimal DailyCapacityKg { get; set; }

        // 🔥 Đổi tên cho rõ đơn vị — đây là số item, không phải kg
        public decimal AssignedTodayCount { get; set; } = 0;

        // 🆕 Thêm để biết khi nào cần reset
        public DateOnly LastResetDate { get; set; } = DateOnly.MinValue;
    }
}