using System;
namespace SmartParking.Entities.Analysis;
public class ShiftAnalysis
{
    public Guid KeeperId { get; set; }
    public int ShiftId { get; set; }
    public int GarageId { get; set; }
    public string KeeperName { get; set; }
    public string ShiftNameAr { get; set; }
    public string ShiftNameEn { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationNameAr { get; set; }
    public string OrganizationNameEn { get; set; }
    public string GarageNameAr {  get; set; }
    public string GarageNameEn {  get; set; }
    public int NumberOfTransaction { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
