using Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RewardRedemption : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; } = null!;

        public Guid RewardId { get; set; }
        public Reward Reward { get; set; } = null!;

        // snapshot để sau này reward đổi giá vẫn giữ đúng lịch sử
        [Range(1, int.MaxValue)]
        public int PointCostSnapshot { get; set; }
    }
}
