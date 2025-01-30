using Abp.Application.Services;
using SmartParking.DataTable;
using SmartParking.Enums;
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

    Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyMdkhlAsync();
    Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyOrganizationAsync(int organizationId);
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyMdKhlAsync();
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync(int organizationId);

    /// <summary>
    /// Get report of garage transaction for mdkhl
    /// </summary>
    /// <param name="category"></param>
    /// <returns> datatable of garage transaction</returns>
    Task<TableData> ReortGarageTransactionMdkhlAsync(CustomerCategoryStatus category, Guid? tenantId);
    /// <summary>
    /// Get report of payment collected money daily and monthly for organization
    /// </summary>
    /// <returns> datatable of payment collected money daily and monthly </returns>
    Task<TableData> ReortPaymentCollectedMoneyDailyAndMonthlyOrganizationAsync();
    /// <summary>
    /// Get report of payment collected money daily and monthly for mdkhl
    /// </summary>
    /// <returns> datatable of payment collected money daily and monthly </returns>
    Task<TableData> ReortPaymentCollectedMoneyDailyAndMonthlyMdkhlAsync();
    /// <summary>
    /// Get count of customers and visitors and employees
    /// </summary>
    /// <returns> count of customers and visitors and employees</returns>
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees,int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesMdkhl();
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesOrganization(int organizationId);
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync();
    Task<TableData> ReortGarageTransactionCustomerIdAsync(Guid CustomerId);
    Task<TableData> ReortGarageStatusDateAsync(DateTime accurateDate); 
    Task<TableData> ReortGarageStatusRangeAsync(DateTime startDate, DateTime endDate);
}
