using System.ComponentModel.DataAnnotations;

namespace Chat.Client.Models;

public class RegisterForm
{
    [Required]
    [StringLength(maximumLength: 30, ErrorMessage = "First name is too long.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(maximumLength: 30, ErrorMessage = "Last name is too long.")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(maximumLength: 50, ErrorMessage = "Email is too long.")]
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}