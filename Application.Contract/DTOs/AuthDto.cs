using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    //Login
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

    //Register
    public class RegisterRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!; // Citizen | Enterprise

        [Required]
        public string FullName { get; set; } = default!;

        [Required, Phone]
        public string Phone { get; set; } = default!;

        public bool IsActive { get; set; } = true; // Citizen: true, Enterprise: BE override false

        public EnterpriseRegisterInfoDto? EnterpriseInfo { get; set; } // chỉ required khi role=Enterprise
    }

    public class EnterpriseRegisterInfoDto
    {
        [Required] public string EnterpriseName { get; set; } = default!;
        [Required] public string TaxCode { get; set; } = default!;
        [Required] public string Address { get; set; } = default!;
        [Required] public string LegalRepresentative { get; set; } = default!;
        [Required] public string RepresentativePosition { get; set; } = default!;
        [Required] public Guid EnvironmentLicenseFileId { get; set; } // hoặc string key/path
    }
    public class RegisterResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }

    //Login
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiredAt { get; set; }
    }

    //Token
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}
