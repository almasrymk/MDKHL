using SmartParking.Enums;
using System;

namespace SmartParking.Organizations;
public class TransactionOrganizationDto
{
    public int Id { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public Status? Status { get; set; }
    public string Code { get; set; }
    public decimal FirstHourPrice { get; set; }
    public decimal SecondHourPrice { get; set; }
    public decimal ThirdHourPrice { get; set; }
    public decimal AdditionalHourPrice { get; set; }
    public decimal FullDayPrice { get; set; }
    public bool IsNumberOfHoursExceeded { get; set; }
    public int NumberOfHours { get; set; }
    public bool IsClose { get; set; }
    public TimeOnly ClosingHoursStart { get; set; }
    public TimeOnly ClosingHoursEnd { get; set; }
    public Guid TenantId { get; set; }
    public string District { get; set; }
    public int ActiveCameras { get; set; }
    public int ParkingSlots { get; set; }
    public int FreeMinutes { get; set; }
    public int Graceperiod { get; set; }
    public bool HasFreeSlots { get; set; }
    public bool HasPackageOnly { get; set; }
    public string PaymentCode { get; set; }
    public bool DispenserTransaction { get; set; }
    public bool CameraTransaction { get; set; }
    public bool UserAccess { get; set; }
    public bool VehicleAccess { get; set; }


}
