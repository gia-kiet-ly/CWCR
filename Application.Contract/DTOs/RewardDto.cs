using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class RewardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int PointCost { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
    }

    public class CreateRewardDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int PointCost { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    public class RewardRedemptionDto
    {
        public Guid Id { get; set; }
        public Guid RewardId { get; set; }
        public string RewardName { get; set; } = default!;
        public int PointCostSnapshot { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
    }

    public class UpdateRewardDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int? PointCost { get; set; }

        // set thẳng stock (admin quản lý số lượng)
        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AdjustRewardStockDto
    {
        // +10 nhập thêm, -3 trừ bớt
        public int Delta { get; set; }
    }
}