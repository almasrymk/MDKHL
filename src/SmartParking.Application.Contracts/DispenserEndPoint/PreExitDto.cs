using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.DispenserEndPoint;

public class PreExitDto
{
    public string ActTime { get; set; }
    public string ArriveTime { get; set; }
    public string BusinessType { get; set; }
    public string CarAccessCurId { get; set; }
    public string CarImgData { get; set; }
    public string CarNum { get; set; }
    public string CarNumcolor { get; set; }
    public string CarPlateImgData { get; set; }
    public string CarType { get; set; }
    public DiscountMoneyDto DiscountMoney { get; set; }
    public string LaneNO { get; set; }
    public string LaneType { get; set; }
    public string LeaveTime { get; set; }
    public string ParkingLot { get; set; }
    public string ParkingLotCode { get; set; }
    public string ParkingTimeLength { get; set; }
    public string PayMoney { get; set; }
    public string PaymentType { get; set; }
    public int PushType { get; set; }
    public string SluiceDevChnId { get; set; }
    public string SluiceDevChnName { get; set; }
    public string TicketID { get; set; }
    public string TotRemianNum { get; set; }
    public string VehicleClass { get; set; }
    public string VttDevChnId { get; set; }
}
public class DiscountMoneyDto
{
    public List<MoneyDto> Money { get; set; }
}
public class MoneyDto
{
    public string Name { get; set; }
    public string Remark { get; set; }
    public double Value { get; set; }
}