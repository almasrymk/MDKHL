
using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SmartParking.CustomerPackages;
using SmartParking.Cutomers;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Authentication;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.Filters;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.Payments;
using SmartParking.PDF;
using SmartParking.Repositories;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParking.Analysis;

public class AnalysisAppService : SmartParkingAppService, IAnalysisAppService
{
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IPackageAnalysisRepository _packageAnalysisRepository;
    private readonly IRepository<Organization, int> _orgRepo;
    private readonly IRepository<Customer, Guid> _custeRepo;
    private readonly IDataFilter _datafilter;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IGarageTransactionAnalysisRepository _garageTransactionAnalysis;
    private readonly IGarageAnalysisRepository _garageAnalysis;
    private readonly IShiftAnalysisRepository _shiftAnalysisRepository;
    private readonly IMyUserRepository _myUserRepository;
    private readonly IPaymentAnalysisRepository _paymentAnalysisRepository;
    private readonly IOrganizationsAppService _organizationsAppService;

    private class PaymentGroup
    {
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public double TotalAmount { get; set; }
        public PaymentCategory PaymentCategory { get; set; }
    }

    private class FinalResult
    {
        public DateTime Day { get; set; }
        public string DayOfWeek { get; set; }
        public double AllTotalAmount { get; set; }
        public double PackageTotalAmount { get; set; }
        public double InvoiceTotalAmount { get; set; }


    }
    public AnalysisAppService(
        IRepository<Customer, Guid> custeRepo,
        IRepository<Organization, int> orgRepo,
        IRepository<Vehicle, int> VehicleRepository,
        IDataFilter datafilter,
        IRepository<Garage, int> GarageRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<CustomerPackage, int> customerPackageRepository,
        IGarageTransactionAnalysisRepository garageTransactionAnalysis,
        IGarageAnalysisRepository garageAnalysis,
        IPaymentAnalysisRepository paymentAnalysisRepository,
        IOrganizationsAppService organizationsAppService,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IPackageAnalysisRepository packageAnalysisRepository,
        IShiftAnalysisRepository shiftAnalysisRepository,
        IMyUserRepository myUserRepository)
    {
        _custeRepo = custeRepo;
        _orgRepo = orgRepo;
        _datafilter = datafilter;
        _garageRepository = GarageRepository;
        _paymentRepository = paymentRepository;
        this._customerPackageRepository = customerPackageRepository;
        _garageTransactionAnalysis = garageTransactionAnalysis;
        _garageAnalysis = garageAnalysis;
        _paymentAnalysisRepository = paymentAnalysisRepository;
        _organizationsAppService = organizationsAppService;
        _garageTransactionRepository = garageTransactionRepository;
        _packageAnalysisRepository = packageAnalysisRepository;
        _vehicleRepository = VehicleRepository;
        _shiftAnalysisRepository = shiftAnalysisRepository;
        _myUserRepository = myUserRepository;
    }

    public async Task<AnalysisNamesCountDto> GetNumberOfCustomerForOrganizations()
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _orgRepo.GetQueryableAsync();
            query = query.Include(x => x.Customers);
            var result = query.Select(x => new Organization { EnglishName = x.EnglishName }).ToList();
            var result2 = query.Select(x => new Organization { Customers = x.Customers }).ToList();
            AnalysisNamesCountDto namesCountDto = new AnalysisNamesCountDto();
            namesCountDto.Names = ObjectMapper.Map<List<Organization>, List<string>>(result);
            namesCountDto.Count = ObjectMapper.Map<List<Organization>, List<double>>(result2);
            return namesCountDto;
        }



    }

    public async Task<AnalysisNamesCountDto> GetPackageAndNumberOfOrganizationsThatSubscribed()
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _orgRepo.GetQueryableAsync();
            query = query.Include(x => x.Subscription);
            query = query.OrderByDescending(org => org.SubscriptionId);
            var result = query.GroupBy(org => org.SubscriptionId)
                .Select(x => (double)x.Sum(o => o.Id)).ToList();
            var result2 = query
                .Select(org => org.Subscription.EnglishName).ToList();

            AnalysisNamesCountDto namesCountDto = new AnalysisNamesCountDto();
            namesCountDto.Names = result2;
            namesCountDto.Count = result;
            return namesCountDto;
        }
    }

    ///<inheritdoc/>
    public async Task<AnalysisNamesCountDto> GetNumberOfOrganizationForLicenseExpiryDate(int yearsNumber)
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _orgRepo.GetQueryableAsync();
            var targetDate = DateOnly.FromDateTime(DateTime.Now).AddYears(yearsNumber);

            query = query
               .Where(o => o.CRExpiryDate <= targetDate);
            var result = query.Select(x => new Organization { EnglishName = x.EnglishName, CRExpiryDate = x.CRExpiryDate }).ToList();
            AnalysisNamesCountDto organizationCustomersDto = new AnalysisNamesCountDto();
            organizationCustomersDto.Names = ObjectMapper.Map<List<Organization>, List<string>>(result);
            organizationCustomersDto.Count.Add(result.Count);
            return organizationCustomersDto;
        }
    }

    public async Task<GetNumberOfParkingAndEmptyParkingDto> GetNumberOfParkingSlotsAndEmptySolts(int organizationId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _garageRepository.GetQueryableAsync();
            query = query
                .Where(_ => _.OrganizationId == organizationId);
            query = query.Where(_ => _.Status == Status.Active);
            // Calculate sums
            var result = new GetNumberOfParkingAndEmptyParkingDto
            {
                NumberOfParking = query.Sum(g => g.CurrnetCapicity),
                NumberOfEmptyParking = query.Sum(g => g.MaxCapacity - g.CurrnetCapicity),
                NumberOfCustomerEmptyParking = query.Sum(c => c.MaxSlotsForCustomers - c.CurrnetCapacityForCustomers),
                NumberOfCustomerParking = query.Sum(c => c.CurrnetCapacityForCustomers),
                NumberOfEmployeeEmptyParking = query.Sum(c => c.MaxSlotsForEmployess - c.CurrnetCapacityEmployees),
                NumberOfEmployeeParking = query.Sum(c => c.CurrnetCapacityEmployees),
                NumberOfVisitorParking = query.Sum(c => c.CurrnetCapacityForFree),
                NumberOfVisitorEmptyParking = query.Sum(c => c.MaxSlotsForFree - c.CurrnetCapacityForFree),
                NumberOfCustomerReservedParking = query.Sum(c => c.MaxSlotsForCustomers),
                NumberOfEmployeeReservedParking = query.Sum(c => c.MaxSlotsForEmployess),
                NumberOfVistiorReservedParking = query.Sum(c => c.MaxSlotsForFree),

            };

            return result;
        }

    }


    public async Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyMdkhlAsync()
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            DateTime now = DateTime.Now;
            DateTime oneWeekAgo = now.AddDays(-7);
            var query = await _paymentRepository.GetQueryableAsync();

            var paymentsInLastWeek = await GetPaymentsInLastWeek(query, oneWeekAgo);
            var allDays = GetAllDays(oneWeekAgo);
            var culture = GetCultureInfo();

            var finalResult = AggregatePaymentsByDay(allDays, paymentsInLastWeek, culture);

            return PrepareResult(finalResult, culture);
        }

    }


    public async Task<(string[] Days, double[] AllTotalAmounts, double[] InvoiceTotalAmounts, double[] PackageTotalAmounts)> CalculateWeeklyCollectedMoneyOrganizationAsync(int organizationId)
    {
        DateTime now = DateTime.Now;
        DateTime oneWeekAgo = now.AddDays(-7);
        var query = await _paymentRepository.GetQueryableAsync();
        query = query.Where(_ => _.OrganizationId == organizationId);

        var paymentsInLastWeek = await GetPaymentsInLastWeek(query, oneWeekAgo);
        var allDays = GetAllDays(oneWeekAgo);
        var culture = GetCultureInfo();

        var finalResult = AggregatePaymentsByDay(allDays, paymentsInLastWeek, culture);

        return PrepareResult(finalResult, culture);
    }

    private async Task<List<PaymentGroup>> GetPaymentsInLastWeek(IQueryable<Payment> query, DateTime oneWeekAgo)
    {
        var result =
            await query
            .Where(payment => payment.CreationTime >= oneWeekAgo.Date)
            .GroupBy(payment => new { payment.CreationTime.Date, payment.PaymentCategory })
            .Select(group => new PaymentGroup
            {
                Date = group.Key.Date,
                PaymentCategory = group.Key.PaymentCategory,
                DayOfWeek = group.Key.Date.DayOfWeek,
                TotalAmount = group.Sum(payment => (double)payment.Amount)
            })
            .ToListAsync();
        return result;
    }

    private List<DateTime> GetAllDays(DateTime oneWeekAgo)
    {
        return Enumerable.Range(0, 8)
            .Select(i => oneWeekAgo.AddDays(i).Date)
            .ToList();
    }

    private CultureInfo GetCultureInfo()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        return currentCulture.Name == "ar" ? new CultureInfo("ar") : new CultureInfo("en");
    }

    private List<FinalResult> AggregatePaymentsByDay(List<DateTime> allDays, List<PaymentGroup> payments, CultureInfo culture)
    {
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

        var finalResult = allDays
            .GroupJoin(
                payments,
                day => day,
                payment => payment.Date,
                (day, paymentGroup) => new FinalResult
                {
                    Day = day,
                    DayOfWeek = day.ToString("dddd"),
                    AllTotalAmount = paymentGroup.Sum(p => p.TotalAmount),
                    InvoiceTotalAmount = paymentGroup.Where(p => p.PaymentCategory == PaymentCategory.PaymentParking).Sum(p => p.TotalAmount),
                    PackageTotalAmount = paymentGroup.Where(p => p.PaymentCategory == PaymentCategory.Package).Sum(p => p.TotalAmount)
                })
            .GroupBy(d => d.DayOfWeek)
            .Select(g => g.OrderByDescending(d => d.Day).First())
            .OrderBy(d => ((int)d.Day.DayOfWeek - (int)firstDayOfWeek + 7) % 7)
            .ToList();
        return finalResult;
    }

    private (string[] days, double[] allTotalAmounts, double[] invoiceTotalAmounts, double[] packageTotalAmounts) PrepareResult(List<FinalResult> finalResult, CultureInfo culture)
    {
        string[] days = finalResult.Select(r => r.DayOfWeek).ToArray();
        double[] allTotalAmounts = finalResult.Select(r => (double)r.AllTotalAmount).ToArray();
        double[] invoiceTotalAmounts = finalResult.Select(r => (double)r.InvoiceTotalAmount).ToArray();
        double[] packageTotalAmounts = finalResult.Select(r => (double)r.PackageTotalAmount).ToArray();

        if (culture.Name == "ar")
        {
            Array.Reverse(days);
            Array.Reverse(allTotalAmounts);
            Array.Reverse(invoiceTotalAmounts);
            Array.Reverse(packageTotalAmounts);
        }

        return (days, allTotalAmounts, invoiceTotalAmounts, packageTotalAmounts);
    }

    public async Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyMdKhlAsync()
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            using (DataFilter.Disable<IMultiTenant>())
            {
                return await GetCollectMoneyToday(query);
            }
        }

    }
    public async Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync()
    {

        var query = await _paymentRepository.GetQueryableAsync();
        using (DataFilter.Disable<IMultiTenant>())
        {
            return await GetCollectMoneyToday(query);
        }

    }

    private async Task<(string? Day, double? totalAmounts)> GetCollectMoneyToday(IQueryable<Payment> query)
    {
        DateTime now = DateTime.Now;
        var culture = GetCultureInfo();
        query = query.Where(payment => payment.CreationTime.DayOfYear == now.DayOfYear);
        var result = await query.FirstOrDefaultAsync();
        var finalResult = new FinalResult
        {

            DayOfWeek = result?.CreationTime.ToString("dddd", culture),
            AllTotalAmount = query.Sum(payment => (double)payment.Amount)
        };
        return (finalResult?.DayOfWeek?.ToString(), finalResult?.AllTotalAmount);
    }

    public async Task<(string? Day, double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync(int organizationId)
    {
        var query = await _paymentRepository.GetQueryableAsync();
        query = query.Where(_ => _.OrganizationId == organizationId);
        return await GetCollectMoneyToday(query);
    }

    public async Task<List<GarageTransactionAnalysis>> ReportGarageTransactionMdkhlAsync(CustomerCategoryStatus category, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {

            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysis(category, tenantId);
            return result.ToList();
        }

    }


    public async Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForMDKHLPlateAsync(string PlateNumber)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            return await ReportGarageTransactionForOrganization(PlateNumber, null);
        }

    }

    public async Task<List<GarageTransactionAnalysis>> ReportGarageTransactionForOrganization(string PlateNumber, Guid? tenantId)
    {
        var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisPlate(PlateNumber, tenantId);
        return result;
    }

    public async Task<List<GarageTransactionAnalysis>> ReportGarageTransactionDateCustomerIdAsync(Guid CustomerId, DateTime startDate, DateTime endDate)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisDateCustomerId(CustomerId, startDate, endDate);
            return result;
        }

    }
    public async Task<List<GarageTransactionAnalysis>> GetGarageTransactionsAnalysisDateCustomerNameAsync(string userName, DateTime startDate, DateTime endDate, CustomerCategoryStatus category, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisDateCustomerName(userName, startDate, endDate, category, tenantId);
            return result;
        }

    }

    public async Task<PagedResultDto<GarageTransactionAnalysis>> ReportGarageTransactionAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<GarageTransactionAnalysis>.BuildFilterExpression(input.filters);

            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisReportAsync(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);
            return new PagedResultDto<GarageTransactionAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }

    public async Task<PagedResultDto<PackagePaymentAnalysis>> ReportPackagePaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<PackagePaymentAnalysis>.BuildFilterExpression(input.filters);

            var result = await _packageAnalysisRepository.GetPackagePaymentAnalysis(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);
            return new PagedResultDto<PackagePaymentAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }
    public async Task<List<GarageTransactionAnalysis>> ReportGarageTransactionCustomerIdAsync(Guid CustomerId, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisCustomerId(CustomerId, tenantId);
            return result;
        }

    }
    public async Task<PagedResultDto<CustomerPackageDto>> ReportCustomerPackage(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _customerPackageRepository.GetQueryableAsync();
            query = query.Include(x => x.Customer).ThenInclude(x => x.IdentityUser);
            var resultJoin = from CustomerPackage in query
                             select new CustomerPackageDto
                             {
                                 ActivationDate = DateOnly.FromDateTime(CustomerPackage.ActivationDateOfPackage.Value),
                                 ActivationTime = TimeOnly.FromDateTime(CustomerPackage.ActivationDateOfPackage.Value),
                                 ActivationDateOfPackage = CustomerPackage.ActivationDateOfPackage.Value,
                                 ArPlateNumber = CustomerPackage.ArPlateNumber,
                                 CustomerCategory = CustomerPackage.CustomerCategory,
                                 CustomerId = CustomerPackage.CustomerId,
                                 CustomerPackageStatus = CustomerPackage.CustomerPackageStatus,
                                 DectivationDateOfPackage = CustomerPackage.DectivationDateOfPackage.Value,
                                 DescriptionOfPackage = CustomerPackage.DescriptionOfPackage,
                                 DurationOfPackage = CustomerPackage.DurationOfPackage,
                                 EnPlateNumber = CustomerPackage.EnPlateNumber,
                                 ExpiryDate = CustomerPackage.ExpiryDate,
                                 ExpiryDateOfPackage = DateOnly.FromDateTime(CustomerPackage.ExpiryDate),
                                 ExpiryTimeOfPackage = TimeOnly.FromDateTime(CustomerPackage.ExpiryDate),
                                 PackageArabicName = CustomerPackage.PackageArabicName,
                                 PackageEnglishName = CustomerPackage.PackageEnglishName,
                                 PriceOfPackage = CustomerPackage.PriceOfPackage,
                                 publishedOfPackage = CustomerPackage.publishedOfPackage,
                                 PublishFromOfPackage = CustomerPackage.PublishFromOfPackage.Value,
                                 PublishToOfPackage = CustomerPackage.PublishToOfPackage.Value,
                                 TenantId = CustomerPackage.TenantId,
                                 CustomerArabicName = CustomerPackage.Customer.ArabicName,
                                 CustomerEnglishName = CustomerPackage.Customer.IdentityUser.Name
                             };
            resultJoin = CheckFilter(input, resultJoin);
            resultJoin = CheckTenant(tenantId, resultJoin);
            resultJoin = ApplySort(input, resultJoin);
            var totalCount = await AsyncExecuter.CountAsync(resultJoin);
            resultJoin = resultJoin.Skip(input.SkipCount).Take(input.MaxResultCount);
            var result = await AsyncExecuter.ToListAsync(resultJoin);
            return new PagedResultDto<CustomerPackageDto>
            {
                TotalCount = totalCount,
                Items = result
            };
        }
    }

    private static IQueryable<CustomerPackageDto> CheckFilter(PagedAndSortedAndFilteredResultRequestDto input, IQueryable<CustomerPackageDto> query)
    {
        if (input.filters.Count > 0)
        {
            var filterExpression = BuildExpression<CustomerPackageDto>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                query = query.Where(filterExpression);
            }
        }

        return query;
    }



    private static IQueryable<CustomerPackageDto> CheckTenant(Guid? tenantId, IQueryable<CustomerPackageDto> query)
    {
        if (tenantId.HasValue)
            query = query.Where(_ => _.TenantId == tenantId.Value);
        return query;
    }

    private static IQueryable<CustomerPackageDto> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<CustomerPackageDto> query)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                query = query.OrderBy(sorting[0] + " descending");
            }
            else
            {
                query = query.OrderBy(input.Sorting);
            }

        }

        return query;
    }
    public async Task<PagedResultDto<PaymentDto>> ReportPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            var organizations = await _orgRepo.GetQueryableAsync();
            query = query.Include(_ => _.Customers).ThenInclude(_ => _.IdentityUser);
            if (tenantId.HasValue)
                query = query.Where(_ => tenantId == tenantId.Value);
            var resultJoin = from payment in query
                             join organization in organizations 
                             on payment.OrganizationId equals organization.Id
                             select new PaymentDto
                             {
                                 Amount = payment.Amount,
                                 Currency = payment.Currency,
                                 Invoice = payment.Invoice == null ? null : payment.Invoice,
                                 CheckoutId = payment.CheckoutId,
                                 Date = DateOnly.FromDateTime(payment.CreationTime),
                                 Time = TimeOnly.FromDateTime(payment.CreationTime),
                                 Customers = payment.Customers ,
                                 PaymentMethod = payment.PaymentMethod,
                                 PaymentNumber = payment.PaymentNumber,
                                 OrganizationArabicName = organization.ArabicName,
                                 OrganizationEnglishName = organization.EnglishName,
                                 PlateNumber = payment.Invoice != null ? payment.Invoice.PlateNumber: payment.EnPlateNumber,
                                 AmountWithoutDiscount = payment.Invoice != null? (payment.Invoice.InvoicePrice * 1.15m) : 0,
                                 QrCodeInvoice = payment.QrCodeInvoice,
                                 CreationTime = payment.CreationTime.ToString(),
                                 PaymentCategory = payment.PaymentCategory,
                                 InvoiceId = payment.InvoiceId.HasValue ? payment.InvoiceId.Value : null,
                             };
            resultJoin = ApplySortPayment(input, resultJoin);
            resultJoin = ApplyFilterPayment(input, resultJoin);
            var totalCount = await AsyncExecuter.CountAsync(resultJoin);
            resultJoin = resultJoin.Skip(input.SkipCount).Take(input.MaxResultCount);
            var result = await AsyncExecuter.ToListAsync(resultJoin);
            return new PagedResultDto<PaymentDto>
            {
                Items = result,
                TotalCount = totalCount
            };
       
        }

    }
    public async Task<PagedResultDto<PaymentDto>> ReportCustomerPayment(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            var organizations = await _orgRepo.GetQueryableAsync();
            query = query.Include(_ => _.Customers).ThenInclude(_ => _.IdentityUser);
            if (tenantId.HasValue)
                query = query.Where(_ => tenantId == tenantId.Value);
            var resultJoin = from payment in query
                             where payment.CustomerId != null
                             join organization in organizations
                             on payment.OrganizationId equals organization.Id
                             select new PaymentDto
                             {
                                 Amount = payment.Amount,
                                 Currency = payment.Currency,
                                 Invoice = payment.Invoice,
                                 CheckoutId = payment.CheckoutId,
                                 Date = DateOnly.FromDateTime(payment.CreationTime),
                                 Time = TimeOnly.FromDateTime(payment.CreationTime),
                                 Customers = payment.Customers,
                                 PaymentMethod = payment.PaymentMethod,
                                 PaymentNumber = payment.PaymentNumber,
                                 OrganizationArabicName = organization.ArabicName,
                                 OrganizationEnglishName = organization.EnglishName,
                                 PlateNumber = payment.Invoice != null ? payment.Invoice.PlateNumber: payment.EnPlateNumber,
                                 AmountWithoutDiscount = payment.Invoice != null ? (payment.Invoice.InvoicePrice * 1.15m) : 0,
                                 QrCodeInvoice = payment.QrCodeInvoice,
                                 CreationTime = payment.CreationTime.ToString(),
                                 PaymentCategory = payment.PaymentCategory,
                                 InvoiceId = payment.InvoiceId
                             };
          //  resultJoin = CheckTenantPayment(tenantId, resultJoin);
            resultJoin = ApplySortPayment(input, resultJoin);
            resultJoin = ApplyFilterPayment(input, resultJoin);
            var totalCount = await AsyncExecuter.CountAsync(resultJoin);
            resultJoin = resultJoin.Skip(input.SkipCount).Take(input.MaxResultCount);
            var result = await AsyncExecuter.ToListAsync(resultJoin);
            return new PagedResultDto<PaymentDto>
            {
                Items = result,
                TotalCount = totalCount
            };
        }

    }

    public async Task<PagedResultDto<PaymentDto>> ReportCustomerPaymentForPaymentMethod(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            var organizations = await _orgRepo.GetQueryableAsync();

            query = query.Include(_ => _.Customers).ThenInclude(_ => _.IdentityUser);
            if (tenantId.HasValue)
                query = query.Where(_ => tenantId == tenantId.Value);
            var resultJoin = from payment in query
                             where payment.PaymentMethod == PaymentMethod.Cash
                             join organization in organizations
                             on payment.OrganizationId equals organization.Id
                             select new PaymentDto
                             {
                                 Amount = payment.Amount,
                                 Currency = payment.Currency,
                                 Invoice = payment.Invoice,
                                 CheckoutId = payment.CheckoutId,
                                 Date = DateOnly.FromDateTime(payment.CreationTime),
                                 Time = TimeOnly.FromDateTime(payment.CreationTime),
                                 Customers = payment.Customers,
                                 PaymentMethod = payment.PaymentMethod,
                                 PaymentNumber = payment.PaymentNumber,
                                 OrganizationArabicName = organization.ArabicName,
                                 OrganizationEnglishName = organization.EnglishName,
                                 PlateNumber = payment.Invoice != null ? payment.Invoice.PlateNumber : payment.EnPlateNumber,
                                 AmountWithoutDiscount = payment.Invoice !=null ? (payment.Invoice.InvoicePrice * 1.15m) : 0,
                                 QrCodeInvoice = payment.QrCodeInvoice,
                                 CreationTime = payment.CreationTime.ToString(),
                                 PaymentCategory = payment.PaymentCategory,
                                 InvoiceId = payment.InvoiceId

                             };

            //resultJoin = CheckTenantPayment(tenantId, resultJoin);
            resultJoin = ApplySortPayment(input, resultJoin);
            resultJoin = ApplyFilterPayment(input, resultJoin);
            var totalCount = await AsyncExecuter.CountAsync(resultJoin);
            resultJoin = resultJoin.Skip(input.SkipCount).Take(input.MaxResultCount);
            var result = await AsyncExecuter.ToListAsync(resultJoin);
            return new PagedResultDto<PaymentDto>
            {
                Items = result,
                TotalCount = totalCount
            };
        }

    }

    private static IQueryable<PaymentDto> ApplyFilterPayment(PagedAndSortedAndFilteredResultRequestDto input, IQueryable<PaymentDto> query)
    {
        if (input.filters.Count > 0)
        {
            var filterExpression = BuildExpression<PaymentDto>.BuildFilterExpression(input.filters);
            if (filterExpression is not null)
            {
                query = query.Where(filterExpression);
            }
        }

        return query;
    }

    private static IQueryable<PaymentDto> ApplySortPayment(PagedAndSortedAndFilteredResultRequestDto input, IQueryable<PaymentDto> query)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                query = query.OrderBy(sorting[0] + " descending");
            }
            else
            {
                query = query.OrderBy(input.Sorting);
            }

        }

        return query;
    }

    private static IQueryable<PaymentDto> CheckTenantPayment(Guid? tenantId, IQueryable<PaymentDto> query)
    {
        if (tenantId.HasValue)
            query = query.Where(_ => _.TenantId == tenantId.Value);
        return query;
    }

    public async Task<PagedResultDto<GarageAnalysis>> ReportGarageAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<GarageAnalysis>.BuildFilterExpression(input.filters);
            var result = await _garageAnalysis.GetGarageReportAsync(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);
            return new PagedResultDto<GarageAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }
    public async Task<PagedResultDto<ShiftAnalysis>> ReportNumberOfTransactionsWhileKeeperShift(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<ShiftAnalysis>.BuildFilterExpression(input.filters);
            var result = await _shiftAnalysisRepository.ReportNumberOfTransactionsWhileKeeperShift(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);
            return new PagedResultDto<ShiftAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }




    public async Task<PagedResultDto<PaymentUserShift>> ReportPaymentCollectedMoneyWithKeeperAndShiftAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<PaymentUserShift>.BuildFilterExpression(input.filters);
            var result = await _paymentAnalysisRepository.GetPaymentCollectedMoneyWithKeeperAndShiftAnalysis(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);

            return new PagedResultDto<PaymentUserShift>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }



    public async Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesMdkhl()
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _custeRepo.GetQueryableAsync();
            var groupedCounts = await query
                   .Select(g => new
                   {
                       CountOfCustomers = query.Where(c => c.CustomerPackages.Any(_ => _.CustomerCategory == CustomerCategoryStatus.Customer)).Count(),
                       CountOfEmployees = query.Where(c => c.CustomerPackages.Any(_ => _.CustomerCategory == CustomerCategoryStatus.Employee)).Count(),
                       CountOfVisitors = query.Where(c => c.CustomerPackages.Any(_ => _.CustomerCategory == CustomerCategoryStatus.Visitor)).Count(),
                       CountOfNoPackage = query.Where(c => !c.CustomerPackages.Any()).Count()
                   }).FirstOrDefaultAsync();
            if (groupedCounts == null)
            {
                return (0, 0, 0, 0);
            }
            return (groupedCounts.CountOfCustomers, groupedCounts.CountOfVisitors, groupedCounts.CountOfEmployees, groupedCounts.CountOfNoPackage);
        }

    }

    public async Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees, int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesOrganization(int organizationId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _customerPackageRepository.GetQueryableAsync();
            var groupedCounts = await query
             .Where(c => c.OrganizationId == organizationId)
             .GroupBy(c => c.CustomerId)
             .Select(g => new
             {
                 FirstCategory = g.OrderBy(c => c.PackageId).Select(c => c.CustomerCategory).FirstOrDefault(),
                 HasPackage = g.Any(c => c.PackageId.HasValue)
             })
             .GroupBy(_ => 1)
             .Select(g => new
             {
                 CountOfCustomers = g.Count(x => x.FirstCategory == CustomerCategoryStatus.Customer && x.HasPackage),
                 CountOfEmployees = g.Count(x => x.FirstCategory == CustomerCategoryStatus.Employee && x.HasPackage),
                 CountOfVisitors = g.Count(x => x.FirstCategory == CustomerCategoryStatus.Visitor && x.HasPackage),
                 CountOfNoPackage = g.Count(x => !x.HasPackage)
             })
             .FirstOrDefaultAsync();
            if (groupedCounts == null)
            {
                return (0, 0, 0, 0);
            }
            return (groupedCounts.CountOfCustomers, groupedCounts.CountOfVisitors, groupedCounts.CountOfEmployees, groupedCounts.CountOfNoPackage);
        }
    }


    public async Task<PagedResultDto<PaymentAnalysis>> ReportKeeperPaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<PaymentAnalysis>.BuildFilterExpression(input.filters);

            var result = await _paymentAnalysisRepository.GetPaymentForKeeper(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression);
            return new PagedResultDto<PaymentAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }

    public async Task<PagedResultDto<DiscountPaymentAnalysis>> ReportDiscountPaymentAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var filterExpression = BuildExpression<DiscountPaymentAnalysis>.BuildFilterExpression(input.filters);

            var result = await _paymentAnalysisRepository.GetReportDiscount(tenantId, input.SkipCount, input.MaxResultCount, input.Sorting, filterExpression,input.SearchingTerm);
            return new PagedResultDto<DiscountPaymentAnalysis>
            {
                Items = result.Queryable.ToList(),
                TotalCount = (int)result.PageSize
            };
        }

    }

    public async Task<PagedResultDto<VehicleCustomerReportDto>> ReportVehicleCustomerAsync(PagedAndSortedAndFilteredResultRequestDto input, Guid? tenantId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {

            var query = await _vehicleRepository.GetQueryableAsync();
            var userQuery = await _myUserRepository.GetQueryableAsync();
            var packageCustomerQuery = await _customerPackageRepository.GetQueryableAsync();
            var resultJoin = from vehicle in query
                             join packageCustomer in packageCustomerQuery
                             on new { vehicle.ArPlateNumber, vehicle.EnPlateNumber }
                             equals new { packageCustomer.ArPlateNumber, packageCustomer.EnPlateNumber }
                             into vehiclePackageGroup
                             from packageCustomer in vehiclePackageGroup.DefaultIfEmpty()
                             select new VehicleCustomerReportDto
                             {
                                 ArPlateNumber = vehicle.ArPlateNumber,
                                 EnPlateNumber = vehicle.EnPlateNumber,
                                 CustomerEnglishName = vehicle.Customer.IdentityUser.Name,
                                 CustomerArabicName = vehicle.Customer.ArabicName,
                                 BrandArabicName = vehicle.VehicleBrand.ArabicName,
                                 BrandEnglishName = vehicle.VehicleBrand.EnglishName,
                                 ModelArabicName = vehicle.VehicleModel.ArabicName,
                                 ModelEnglishName = vehicle.VehicleModel.EnglishName,
                                 CreatedBy  = userQuery.Where(_ => _.Id == vehicle.CreatorId).FirstOrDefault().Name,
                                 CreationTime = TimeOnly.FromDateTime(vehicle.CreationTime),
                                 CreationDate = DateOnly.FromDateTime(vehicle.CreationTime),
                                 ModifiedBy  = userQuery.Where(_ => _.Id == vehicle.LastModifierId).FirstOrDefault().Name,
                                 ModifiedTime = TimeOnly.FromDateTime((DateTime)vehicle.LastModificationTime),
                                 ModifiedDate = DateOnly.FromDateTime((DateTime)vehicle.LastModificationTime),
                                 ModelYear = vehicle.ModelYear,
                                 TenantId = vehicle.TenantId,
                                 CustomerCategory =  packageCustomer.CustomerCategory == null? CustomerCategoryStatus.None: 
                                 packageCustomerQuery
                                        .Where(_ =>_.ExpiryDate >= DateTime.Now)
                                        .Select(_ => _.CustomerCategory).FirstOrDefault(),

                                 PackageNameArabicName =
                                    packageCustomerQuery
                                        .Where(_ => _.ArPlateNumber == vehicle.ArPlateNumber &&
                                                    _.EnPlateNumber == vehicle.EnPlateNumber && 
                                                    _.ExpiryDate >= DateTime.Now)
                                        .Select(_ => _.PackageArabicName).FirstOrDefault(),
                                PackageNameEnglishName = packageCustomerQuery
                                        .Where(_ => _.ArPlateNumber == vehicle.ArPlateNumber &&
                                                    _.EnPlateNumber == vehicle.EnPlateNumber &&
                                                    _.ExpiryDate >= DateTime.Now)
                                        .Select(_ => _.PackageEnglishName).FirstOrDefault()
                             };
            
            if (tenantId.HasValue)
                resultJoin = resultJoin.Where(_ => _.TenantId == tenantId);
            if (!input.Sorting.IsNullOrEmpty())
            {
                if (input.Sorting.Contains("DESC"))
                {
                    var sorting = input.Sorting.Split(' ').ToList();
                    resultJoin = resultJoin.OrderBy(sorting[0] + " descending");
                }
                else
                {
                    resultJoin = resultJoin.OrderBy(input.Sorting);
                }

            }
            if (input.filters.Count > 0)
            {
                var filterExpression = BuildExpression<VehicleCustomerReportDto>.BuildFilterExpression(input.filters);
                if (filterExpression is not null)
                {
                    resultJoin = resultJoin.Where(filterExpression);
                }
            }
            if(!input.SearchingTerm.IsNullOrEmpty())
            {
                resultJoin = resultJoin.Where(_ => _.CustomerEnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                _.CustomerArabicName.ToLower().Contains(input.SearchingTerm.ToLower())
                );
            }
            var totalCount = await AsyncExecuter.CountAsync(resultJoin);
            resultJoin = resultJoin.Skip(input.SkipCount).Take(input.MaxResultCount);
            var result = await AsyncExecuter.ToListAsync(resultJoin);
            return new PagedResultDto<VehicleCustomerReportDto>
            {
                Items = result,
                TotalCount = totalCount
            };
        }
    }

}
