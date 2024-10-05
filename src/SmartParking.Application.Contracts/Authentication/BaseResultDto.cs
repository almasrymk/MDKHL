using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Authentication;

public class BaseResultDto
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
