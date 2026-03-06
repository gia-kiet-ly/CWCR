using Domain.Base;

namespace Domain.Entities
{
    public class CitizenPoint : BaseEntity
    {
        public Guid CitizenId { get; set; }
        public ApplicationUser Citizen { get; set; } = null!;

        public int TotalPoints { get; set; } = 0;
        public ICollection<CitizenPointHistory> Histories { get; set; } = new List<CitizenPointHistory>();
    }
}