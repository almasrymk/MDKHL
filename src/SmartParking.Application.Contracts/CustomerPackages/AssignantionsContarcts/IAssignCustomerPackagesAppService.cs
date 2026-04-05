using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.Payments;
using SmartParking.Results;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.AssignantionsContarcts;

public interface IAssignCustomerPackagesAppService
{
    /// <summary>
    /// Assigns a package to a customer using the specified package ID and vehicle plate numbers.
    /// </summary>
    /// <param name="packageId">The ID of the package to assign.</param>
    /// <param name="ArabicPlateNumber">The Arabic version of the vehicle plate number.</param>
    /// <param name="EnglishPlateNumber">The English version of the vehicle plate number.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Result"/> indicating the outcome of the assignment.
    /// </returns>
    Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(CreateUpdateCustomerPackageDto createUpdateCustomerPackageDto);
    Task<CustomerPackageDto> UpdateCustomerPackage(int customerPackageId, CreateUpdateCustomerPackageDto input);

}
