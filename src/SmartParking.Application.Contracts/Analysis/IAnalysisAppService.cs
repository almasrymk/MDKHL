using Abp.Application.Services;
using SmartParking.CustomerPackages;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Enums;
using SmartParking.Payments;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Analysis;

/// <summary>
/// Interface for analysis service.
/// </summary>
public interface IAnalysisAppService : IApplicationService
{

    /// <summary>
    /// Gets the number of customers for organizations.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the number of customers in the <see cref="AnalysisNamesCountDto"/>.</returns>
    Task<AnalysisNamesCountDto> GetNumberOfCustomerForOrganizations();

    /// <summary>
    /// Gets the number of organizations that have subscribed to a specific package.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the number of subscribed organizations in the <see cref="AnalysisNamesCountDto"/>.</returns>
    Task<AnalysisNamesCountDto> GetPackageAndNumberOfOrganizationsThatSubscribed();

    /// <summary>
    /// Gets the number of organizations with license expiry dates within a specific range of years.
    /// </summary>
    /// <param name="yearsNumber">The number of years within which the license expiry is to be checked.</param>
    /// <returns>A task that represents the asynchronous operation, containing the number of organizations with license expiry in the <see cref="AnalysisNamesCountDto"/>.</returns>
    Task<AnalysisNamesCountDto> GetNumberOfOrganizationForLicenseExpiryDate(int yearsNumber);

    /// <summary>
    /// Gets the number of parking slots and empty parking slots for a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task that represents the asynchronous operation, containing the parking slot information in <see cref="GetNumberOfParkingAndEmptyParkingDto"/>.</returns>
    Task<GetNumberOfParkingAndEmptyParkingDto> GetNumberOfParkingSlotsAndEmptySolts(int organizationId);

    /// <summary>
    /// Calculates the weekly collected money for MDKHL (Main Data Collection Hub).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the daily money amounts for all, invoice, and package in a tuple format.</returns>
    Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyMdkhlAsync();

    /// <summary>
    /// Calculates the weekly collected money for a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task that represents the asynchronous operation, containing the daily money amounts for all, invoice, and package in a tuple format.</returns>
    Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyOrganizationAsync(int organizationId);

    /// <summary>
    /// Calculates the daily collected money for MDKHL (Main Data Collection Hub).
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the daily total money amount in a tuple format.</returns>
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyMdKhlAsync();

    /// <summary>
    /// Calculates the daily collected money for a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task that represents the asynchronous operation, containing the daily total money amount in a tuple format.</returns>
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync(int organizationId);

    /// <summary>
    /// Reports the garage transaction analysis for MDKHL based on the customer category status and tenant ID.
    /// </summary>
    /// <param name="category">The customer category status.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionMdkhlAsync(CustomerCategoryStatus category, Guid? tenantId);

    /// <summary>
    /// Reports the payment collected money with keeper and shift details, based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PaymentUserShift}"/> result.</returns>
    Task<PagedResultDto<PaymentUserShift>> ReportPaymentCollectedMoneyWithKeeperAndShiftAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Gets the count of customers, visitors, employees, and those with no package for MDKHL.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the count in a tuple format.</returns>
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesMdkhl();

    /// <summary>
    /// Gets the count of customers, visitors, employees, and those with no package for a specific organization.
    /// </summary>
    /// <param name="organizationId">The unique identifier of the organization.</param>
    /// <returns>A task that represents the asynchronous operation, containing the count in a tuple format.</returns>
    Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesOrganization(int organizationId);

    /// <summary>
    /// Calculates the daily collected money for a specific organization.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the daily total money amount in a tuple format.</returns>
    Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync();

    /// <summary>
    /// Reports the garage transaction analysis for a specific customer ID and tenant ID.
    /// </summary>
    /// <param name="CustomerId">The unique identifier of the customer.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionCustomerIdAsync(Guid CustomerId, Guid? tenantId);

    /// <summary>
    /// Reports the garage analysis based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{GarageAnalysis}"/> result.</returns>
    Task<PagedResultDto<GarageAnalysis>> ReportGarageAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports the garage transaction for a specific organization based on the vehicle plate number and tenant ID.
    /// </summary>
    /// <param name="PlateNumber">The plate number of the vehicle.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForOrganization(string PlateNumber, Guid? tenantId);

    /// <summary>
    /// Reports the garage transaction for MDKHL based on the vehicle plate number.
    /// </summary>
    /// <param name="PlateNumber">The plate number of the vehicle.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForMDKHLPlateAsync(string PlateNumber);

    /// <summary>
    /// Reports customer payment information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PaymentDto}"/> result.</returns>
    Task<PagedResultDto<PaymentDto>> ReportCustomerPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports payment information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PaymentDto}"/> result.</returns>
    Task<PagedResultDto<PaymentDto>> ReportPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports garage transaction analysis for a specific customer ID within a date range.
    /// </summary>
    /// <param name="CustomerId">The unique identifier of the customer.</param>
    /// <param name="startDate">The start date of the transaction range.</param>
    /// <param name="endDate">The end date of the transaction range.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> ReportGarageTransactionDateCustomerIdAsync(Guid CustomerId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets garage transactions analysis based on a customer name, date range, and category status.
    /// </summary>
    /// <param name="userName">The username of the customer.</param>
    /// <param name="startDate">The start date of the transaction range.</param>
    /// <param name="endDate">The end date of the transaction range.</param>
    /// <param name="category">The customer category status.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of <see cref="GarageTransactionAnalysis"/> results.</returns>
    Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerNameAsync(string userName, DateTime startDate, DateTime endDate, CustomerCategoryStatus category, Guid? tenantId);

    /// <summary>
    /// Reports garage transaction analysis based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{GarageTransactionAnalysis}"/> result.</returns>
    Task<PagedResultDto<GarageTransactionAnalysis>> ReportGarageTransactionAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports customer package information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{CustomerPackageDto}"/> result.</returns>
    Task<PagedResultDto<CustomerPackageDto>> ReportCustomerPackage(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports package payment information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PackagePaymentAnalysis}"/> result.</returns>
    Task<PagedResultDto<PackagePaymentAnalysis>> ReportPackagePaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports customer payment information for a specific payment method based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PaymentDto}"/> result.</returns>
    Task<PagedResultDto<PaymentDto>> ReportCustomerPaymentForPaymentMethod(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports keeper payment information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{PaymentAnalysis}"/> result.</returns>
    Task<PagedResultDto<PaymentAnalysis>> ReportKeeperPaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports the number of transactions while a keeper shift is ongoing based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{ShiftAnalysis}"/> result.</returns>
    Task<PagedResultDto<ShiftAnalysis>> ReportNumberOfTransactionsWhileKeeperShift(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports discount payment information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{DiscountPaymentAnalysis}"/> result.</returns>
    Task<PagedResultDto<DiscountPaymentAnalysis>> ReportDiscountPaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports vehicle customer information based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{VehicleCustomerReportDto}"/> result.</returns>
    Task<PagedResultDto<VehicleCustomerReportDto>> ReportVehicleCustomerAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Reports the beneficiaries' details based on pagination, sorting, and filtering.
    /// </summary>
    /// <param name="input">The input parameters for pagination, sorting, and filtering.</param>
    /// <param name="tenantId">The unique identifier for the tenant.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="PagedResultDto{CustomerAnalysis}"/> result.</returns>
    Task<PagedResultDto<CustomerAnalysis>> ReportBeneficiariesDetailsAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId);

}
