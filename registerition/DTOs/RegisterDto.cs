using System.ComponentModel.DataAnnotations;

namespace registerition.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [RegularExpression("^(patient|doctor)$", ErrorMessage = "Role must be either 'patient' or 'doctor'.")]
        public string Role { get; set; } = null!;
    }
}
