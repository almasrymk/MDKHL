using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DispenserEndPoint;

public class OpenGateResponseDto : IBaseGateResponseDto
{
    public string ResCode { get; set; } = "0000";
    public string OpenGate { get; set; } = "1";  
    public string ResMsg { get; set; } = "Success";
}
