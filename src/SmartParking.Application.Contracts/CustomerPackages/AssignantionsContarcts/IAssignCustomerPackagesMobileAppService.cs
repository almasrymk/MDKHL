using SmartParking.CustomerPackages.Dtos;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.AssignantionsContarcts;

public interface IAssignCustomerPackagesMobileAppService
{
    Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(int packageId, string ArabicPlateNumber, string EnglishPlateNumber);

}
