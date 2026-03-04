using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Application.Contract.Interfaces.Services;

namespace Application.Services
{
    /// <summary>
    /// Reverse geocode lat/lng -> RegionCode (Quận/Huyện level).
    /// MVP: Nominatim OSM.
    /// Output examples: "HCM-Q1", "HN-BA_DINH", "DN-HAI_CHAU"
    /// </summary>
    public class RegionCodeResolver : IRegionCodeResolver
    {
        private readonly HttpClient _http;

        public RegionCodeResolver(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> ResolveDistrictRegionCodeAsync(decimal latitude, decimal longitude)
        {
            // Nominatim reverse endpoint
            // NOTE: You'll set BaseAddress + User-Agent via DI later (Program.cs).
            var url =
                $"reverse?format=jsonv2&lat={latitude.ToString(CultureInfo.InvariantCulture)}&lon={longitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&zoom=14&addressdetails=1";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // Safety: some deployments forget to set UA in DI, so set a fallback here too
            if (!req.Headers.UserAgent.Any())
                req.Headers.UserAgent.ParseAdd("EcoCollect/1.0 (contact: dev@local)");

            HttpResponseMessage resp;
            try
            {
                resp = await _http.SendAsync(req);
            }
            catch
            {
                return null;
            }

            if (!resp.IsSuccessStatusCode) return null;

            string json;
            try
            {
                json = await resp.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("address", out var address))
                    return null;

                // ---- Province/City ----
                var provinceRaw =
                    GetString(address, "state") ??
                    GetString(address, "city") ??
                    GetString(address, "province");

                // ---- District ----
                // Nominatim keys vary by area. Try common ones in order.
                var districtRaw =
                    GetString(address, "city_district") ??
                    GetString(address, "county") ??
                    GetString(address, "district") ??
                    GetString(address, "suburb") ??
                    GetString(address, "town");

                if (string.IsNullOrWhiteSpace(provinceRaw) || string.IsNullOrWhiteSpace(districtRaw))
                    return null;

                var provinceCode = ToProvinceCodeVN(provinceRaw);
                var districtCode = ToDistrictCodeVN(districtRaw);

                return $"{provinceCode}-{districtCode}";
            }
            catch
            {
                return null;
            }
        }

        private static string? GetString(JsonElement obj, string name)
        {
            if (obj.TryGetProperty(name, out var v))
            {
                if (v.ValueKind == JsonValueKind.String) return v.GetString();
            }
            return null;
        }

        /// <summary>
        /// Convert Vietnamese province/city name to short code.
        /// MVP mapping: HCM, HN, DN; fallback: slug uppercase.
        /// </summary>
        private static string ToProvinceCodeVN(string provinceRaw)
        {
            var s = Normalize(provinceRaw);

            // Common big cities
            if (s.Contains("HO_CHI_MINH") || s.Contains("TP_HO_CHI_MINH") || s.Contains("THANH_PHO_HO_CHI_MINH"))
                return "HCM";
            if (s.Contains("HA_NOI") || s.Contains("THANH_PHO_HA_NOI"))
                return "HN";
            if (s.Contains("DA_NANG") || s.Contains("THANH_PHO_DA_NANG"))
                return "DN";

            // Optional: add more known mappings here if you like.
            // Fallback
            return Shorten(s, 12);
        }

        /// <summary>
        /// Convert district name to code:
        /// - "Quận 1" => "Q1"
        /// - "Quận 10" => "Q10"
        /// - "Huyện Củ Chi" => "CU_CHI" (or "H_CU_CHI" if you prefer)
        /// - "Ba Đình" => "BA_DINH"
        /// </summary>
        private static string ToDistrictCodeVN(string districtRaw)
        {
            var s = Normalize(districtRaw);

            // Handle "QUAN <number>"
            // Normalize turns "Quận 1" -> "QUAN_1"
            if (s.StartsWith("QUAN_"))
            {
                var numPart = s.Substring("QUAN_".Length);
                if (int.TryParse(numPart, out var n))
                    return $"Q{n}";
                // if it's "QUAN_1_..." just take first numeric token
                var firstToken = numPart.Split('_', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (int.TryParse(firstToken, out n))
                    return $"Q{n}";
            }

            // Strip common prefixes
            s = StripPrefix(s, "QUAN_");
            s = StripPrefix(s, "HUYEN_");
            s = StripPrefix(s, "THI_XA_");
            s = StripPrefix(s, "THANH_PHO_"); // sometimes district comes as city

            // Fallback: keep slug uppercase
            return Shorten(s, 24);
        }

        private static string StripPrefix(string s, string prefix)
            => s.StartsWith(prefix) ? s.Substring(prefix.Length) : s;

        /// <summary>
        /// Normalize string: remove diacritics, keep A-Z0-9 and convert spaces to underscore.
        /// </summary>
        private static string Normalize(string input)
        {
            var trimmed = input.Trim();

            // Normalize to decomposed form then remove diacritics
            var formD = trimmed.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);

            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc == UnicodeCategory.NonSpacingMark) continue;

                // Keep letters/digits, convert separators to underscore
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    sb.Append('_');
                }
            }

            // Collapse multiple underscores
            var s = sb.ToString();
            while (s.Contains("__")) s = s.Replace("__", "_");
            s = s.Trim('_');

            return s;
        }

        private static string Shorten(string s, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(s)) return "UNKNOWN";
            return s.Length <= maxLen ? s : s.Substring(0, maxLen);
        }
    }
}