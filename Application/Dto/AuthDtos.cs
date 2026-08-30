using System.ComponentModel.DataAnnotations;

namespace Application.Dto
{
    public record RegisterDto(
        [Required][EmailAddress] string Email,
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one digit, and one special character.")]
        string Password,
        [Required][MaxLength(50)] string FirstName,
        [Required][MaxLength(50)] string LastName
    );

    public record EmailLoginDto(
        [Required][EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponseDto(string AccessToken, string RefreshToken);

    public record RefreshRequestDto([Required] string RefreshToken);

    public record RevokeRequestDto([Required] string RefreshToken);
}
