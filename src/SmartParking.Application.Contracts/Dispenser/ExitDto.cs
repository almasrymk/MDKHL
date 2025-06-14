using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Dispenser;

public class ExitDto
{
    public string ActTime { get; set; }
    public string ArriveTime { get; set; }
    public string CarAccessCurId { get; set; }
    public string? CarImgData { get; set; }
    public string CarNum { get; set; }
    public int? CarNumcolor { get; set; }
    public string? CarPlateImgData { get; set; }
    public string CarType { get; set; }
    public int LaneType { get; set; }
    public string ParkingLot { get; set; }
    public string ParkingLotCode { get; set; }
    public PushType PushType { get; set; }
    public string SluiceDevChnId { get; set; }
    public string SluiceDevChnName { get; set; }
    public int? TotRemianNum { get; set; }
    public string VehicleClass { get; set; }
    public string VttDevChnId { get; set; }
    public string LeaveTime { get; set; }
    public string LaneNO { get; set; } 
}
