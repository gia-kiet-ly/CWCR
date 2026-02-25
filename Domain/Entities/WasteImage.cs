using Core.Enum;
using Domain.Base;
using Domain.Entities;

public class WasteImage : BaseEntity
{
    public Guid WasteReportWasteId { get; set; }
    public WasteReportWaste WasteReportWaste { get; set; } = null!;

    public string ImageUrl { get; set; } = default!;
    public string PublicId { get; set; } = default!;
    public WasteImageType ImageType { get; set; }
}