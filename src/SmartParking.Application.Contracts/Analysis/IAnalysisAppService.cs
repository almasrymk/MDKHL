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


    

}
