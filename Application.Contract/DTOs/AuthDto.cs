using System.ComponentModel.DataAnnotations;

namespace Application.Contract.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

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
    }

    public class RegisterResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public bool RequireEmailVerification { get; set; } = true;
        public string Message { get; set; } = default!;
    }

    public class EmailVerificationResultDto
    {
        public bool Succeeded { get; set; }
        public string Email { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string NextStep { get; set; } = default!;
        // Citizen -> Login
        // Enterprise -> CompleteEnterpriseProfile
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiredAt { get; set; }

        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;

        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }

        public string? EnterpriseApprovalStatus { get; set; }
        public bool CanLogin { get; set; }
        public bool MustCompleteEnterpriseProfile { get; set; }
    }

    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = default!;
    }
}