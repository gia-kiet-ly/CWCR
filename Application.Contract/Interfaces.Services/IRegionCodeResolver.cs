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
        /// <summary>
        /// Existing method - KEEP for backward compatibility
        /// </summary>
        Task<string?> ResolveDistrictRegionCodeAsync(decimal latitude, decimal longitude);

        /// <summary>
        /// NEW: Resolve both Address (full text) and RegionCode in ONE API call
        /// </summary>
        Task<(string? Address, string? RegionCode)> ResolveFullAsync(decimal latitude, decimal longitude);
    }
}