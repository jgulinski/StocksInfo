using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class UserDto
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
    
    [Required]
    public string Password { get; set; }
}