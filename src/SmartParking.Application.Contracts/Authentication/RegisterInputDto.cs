using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class RegisterInputDto
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [Phone]
    public string Phone { get; set; }
    [Required]
    [RegularExpression("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email is not valid.")]
    [EmailAddress]

    public string Email { get; set; }
    [Required]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and one special character, and must be at least 8 characters long.")]
    [PasswordPropertyText]
    public string Password { get; set; }
}
