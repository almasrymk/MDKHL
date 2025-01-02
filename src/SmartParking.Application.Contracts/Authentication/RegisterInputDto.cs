using Microsoft.Extensions.Localization;
using SmartParking.Enums;
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
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public IdentificationTypes IdentificationTypes { get; set; }
    public string NationalId { get; set; }
}
