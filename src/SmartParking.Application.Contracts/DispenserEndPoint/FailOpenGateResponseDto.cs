using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DispenserEndPoint;

public class FailOpenGateResponseDto : IBaseGateResponseDto
{
    public string ResCode { get; set; }  
    public string ResMsg { get; set; }
    public string OpenGate { get; set; }
    public string CarNum { get; set; }
}
