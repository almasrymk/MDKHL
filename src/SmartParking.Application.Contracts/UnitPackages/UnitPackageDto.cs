using SmartParking.Enums;
using SmartParking.Packages;
using Volo.Abp.Application.Dtos;

namespace SmartParking.UnitPackages;

public class UnitPackageDto : AuditedEntityDto<int>
{
    public PackageDto Package {  get; set; }
    public int NumberOfVehicles { get; set; }
    public int NumberOfSlots { get; set; }

}
