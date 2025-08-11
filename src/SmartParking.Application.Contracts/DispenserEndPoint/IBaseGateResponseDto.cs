using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DispenserEndPoint;

public interface IBaseGateResponseDto
{
    string ResCode { get; set; }
    string OpenGate { get; set; }
    string ResMsg { get; set; }
    string CarNum { get; set; }
}
