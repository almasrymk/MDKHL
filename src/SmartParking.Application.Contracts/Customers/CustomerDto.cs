
using SmartParking.CustomerPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Packages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Cutomers;

public class CustomerDto:AuditedEntityDto<Guid>
{
    public string FullName { get; set; }
    public string UserName { get; set; }
    public string ArabicName { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string NationalId { get; set; }
    public IdentificationTypes identificationTypes { get; set; }
    public List<VehicleDto> Vehicles { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public int OrganizationId { get; set; }
    public bool isActive { get; set; }
    public string CustomerCategory { get; set; }
    public int PackageId { get; set; }
    public Guid IdentityId { get; set; }
    public string _packageEnglishNames;
    public string _packageArabicNames;

    public string PackageEnglishNames
    {
        get => CleanCommas(_packageEnglishNames);
        set => _packageEnglishNames = value;
    }

    public string PackageArabicNames
    {
        get => CleanCommas(_packageArabicNames);
        set => _packageArabicNames = value;
    }

    private string CleanCommas(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input; 

        return input.Trim().Trim(',');
    }

}


