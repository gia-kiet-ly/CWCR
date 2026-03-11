using System.ComponentModel.DataAnnotations;

namespace Core.Enum
{
    public enum RejectReason
    {
        [Display(Name = "Capacity Full")]
        CapacityFull,

        [Display(Name = "Out of Service Area")]
        OutOfServiceArea,

        [Display(Name = "Wrong Waste Type")]
        WrongWasteType,

        [Display(Name = "Image Not Clear")]
        ImageNotClear,

        [Display(Name = "Other")]
        Other
    }
}