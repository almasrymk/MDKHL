using SmartParking.CustomerPackages.Inputs;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.Validations;

public interface ICustomerPackageValidatorAppService
{
    Task<bool> HandleExistingPackagesForVehicleAsync(CreateUpdateCustomerPackageDto customerPackageDto);
}
