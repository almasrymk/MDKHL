using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartParking.BarrierAppService;

public class BarrierRequest
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = "/sp/remote/barrierControl";

    public string? laneNO { get; set; }
    public string? carNum { get; set; }
    public int operatorType { get; set; }
}
