using Castle.MicroKernel.Registration;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Organizations;

public class OrganizationDto
{
    public int Id { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string CommercialRegistrationNumber { get; set; }
    public int NumOfCameras { get; set; }
    public int MaxCapacity { get; set; }
    public DateOnly CRExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly LicenseExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public int CountryId { get; set; }
    public int RegionId { get; set; }
    public string RegionName { get; set; }
    public string RegionNameAr { get; set; }
    public int StateId { get; set; }
    public  int SubscriptionId { get; set; }
    public int CityId { get; set; }
    public string PhoneNum { get; set; }
    public string WhatsAppNum { get; set; }
    public string ContactMail { get; set; }
    public Status? Status { get; set; }
    public string Code { get; set; }
    public string CountryName { get; set; }
    public string CountryNameAr { get; set; }
    public string StateName { get; set; }
    public string StateNameAr { get; set; }
    public string CityName { get; set; }
    public string CityNameAr { get; set; }
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
    public string Address { get; set; }
    public string OrganizationPicture { get; set; }

}
