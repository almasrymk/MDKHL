using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Analysis;

public interface IAnalysisAppService : IApplicationService
{
    /// <summary>
    /// Method that gets total number of customer for each organization.
    /// </summary>
    /// <returns>List of organizations and number of customers in each</returns>
    Task<AnalysisNamesCountDto> GetNumberOfCustomerForOrganizations();

    /// <summary>
    /// Method that gets Packages and number of organizations subscribed it.
    /// </summary>
    /// <returns>List of Packages and Number of Organizations</returns>
    Task<AnalysisNamesCountDto> GetPackageAndNumberOfOrganizationsThatSubscribed();

    /// <summary>
    /// Retrieves the number of organizations  whose license expiry date falls within a specified number of years.
    /// </summary>
    /// <param name="yearsNumber">The number of years from the current date to check for license expiry.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the 
    /// <see cref="AnalysisNamesCountDto"/> which includes the count of organizations with licenses expiring within the specified period.
    /// </returns>
    Task<AnalysisNamesCountDto> GetNumberOfOrganizationForLicenseExpiryDate(int yearsNumber);

    Task<GetNumberOfParkingAndEmptyParkingDto> GetNumberOfParkingSlotsAndEmptySolts(int organizationId);

}
