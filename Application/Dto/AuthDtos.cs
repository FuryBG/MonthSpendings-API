using Application.Resources;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto
{
    public record RegisterDto(
        [Required][EmailAddress] string Email,
        [Required]
        [MinLength(8, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.AuthPasswordTooShort))]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.AuthPasswordTooWeak))]
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
