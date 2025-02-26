using Abp.Application.Services;
using SmartParking.CustomerPackages;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

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
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionMdkhlAsync(CustomerCategoryStatus category, Guid? tenantId);
    Task<PagedResultDto<PaymentUserShift>> ReportPaymentCollectedMoneyWithKeeperAndShiftAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    /// <summary>
    /// Get count of customers and visitors and employees
    /// </summary>
    /// <returns> count of customers and visitors and employees</returns>
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees,int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesMdkhl();
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesOrganization(int organizationId);
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync();
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionCustomerIdAsync(Guid CustomerId);
    Task<PagedResultDto<GarageAnalysis>> ReportGarageAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForOrganization(string PlateNumber);
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForMDKHLPlateAsync(string PlateNumber);
    Task<PagedResultDto<PaymentDto>> ReportCustomerPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    Task<PagedResultDto<PaymentDto>> ReportPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionDateCustomerIdAsync(Guid CustomerId, DateTime startDate, DateTime endDate);
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerNameAsync(string userName, DateTime startDate, DateTime endDate, CustomerCategoryStatus category, Guid? tenantId);  
    Task<PagedResultDto<GarageTransactionAnalysis>> ReportGarageTransactionAsync(PagedAndSortedAndFilteredResultRequestDto input,Guid? tenantId);
    Task<PagedResultDto<CustomerPackageDto>> ReportCustomerPackage(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    Task<PagedResultDto<PackagePaymentAnalysis>> ReportPackagePaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
    Task<PagedResultDto<PaymentDto>> ReportCustomerPaymentForPaymentMethod(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    Task<PagedResultDto<PaymentAnalysis>> ReportKeeperPaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);
}
