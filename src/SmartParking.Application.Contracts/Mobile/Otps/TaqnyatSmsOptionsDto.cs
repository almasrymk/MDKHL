using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Mobile.Otps;

public class TaqnyatSmsOptionsDto
{
    public string ApiKey { get; set; } = "363d063620593ca455fcfbff530f32d5";
    public string ApiUrl { get; set; } = "https://api.taqnyat.sa/v1/messages";
    public string SenderName { get; set; } = "Rwadalmdina";
}
