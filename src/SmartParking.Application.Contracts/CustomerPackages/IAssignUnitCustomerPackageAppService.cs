using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;

public interface IAssignUnitCustomerPackageAppService
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

}
