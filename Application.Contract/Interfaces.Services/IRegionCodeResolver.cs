using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    /// <summary>
    /// Resolve RegionCode (Quận/Huyện level) from latitude/longitude.
    /// Example: "HCM-Q1", "HN-BA_DINH"
    /// </summary>
    public interface IRegionCodeResolver
    {
        Task<string?> ResolveDistrictRegionCodeAsync(decimal latitude, decimal longitude);
    }
}
