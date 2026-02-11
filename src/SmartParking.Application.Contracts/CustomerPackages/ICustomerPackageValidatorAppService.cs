using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;

public interface ICustomerPackageValidatorAppService
{
    Task<bool> HandleExistingPackagesForVehicleAsync(CreateUpdateCustomerPackageDto customerPackageDto);
}
