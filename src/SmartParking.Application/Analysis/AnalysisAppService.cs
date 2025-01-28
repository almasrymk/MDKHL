using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Authentication;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
using SmartParking.Organizations;
using SmartParking.PDF;
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Analysis;

public class AnalysisAppService : SmartParkingAppService, IAnalysisAppService
{
    private readonly IRepository<Invoice, int> _invoiceRepo;
    private readonly IRepository<Organization, int> _orgRepo;
    private readonly IRepository<Customer, Guid> _custeRepo; 
    private readonly IDataFilter _datafilter;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IGarageTransactionAnalysisRepository _garageTransactionAnalysis;
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
    public AnalysisAppService(IRepository<Customer, Guid> custeRepo,
        IRepository<Organization, int> orgRepo, IRepository<Invoice, int> invoiceRepo,
        IDataFilter datafilter,IRepository<Garage,int> GarageRepository,
        IRepository<Payment,Guid> paymentRepository,
        IRepository<CustomerPackage,int> customerPackageRepository,
        IGarageTransactionAnalysisRepository garageTransactionAnalysis,
        IPaymentAnalysisRepository paymentAnalysisRepository,
        IOrganizationsAppService organizationsAppService)
    {
        _custeRepo = custeRepo;
        _orgRepo = orgRepo;
        _invoiceRepo = invoiceRepo;
        _datafilter = datafilter;
        _garageRepository = GarageRepository;
        _paymentRepository = paymentRepository;
        this._customerPackageRepository = customerPackageRepository;
        _garageTransactionAnalysis = garageTransactionAnalysis;
        _paymentAnalysisRepository = paymentAnalysisRepository;
        _organizationsAppService = organizationsAppService;
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
            // Calculate sums
            var result = new GetNumberOfParkingAndEmptyParkingDto
            {
                NumberOfParking = query.Sum(g => g.CurrnetCapicity), 
                NumberOfEmptyParking = query.Sum(g => g.MaxCapacity - g.CurrnetCapicity) ,
                NumberOfCustomerEmptyParking = query.Sum(c => c.MaxSlotsForCustomers - c.CurrnetCapacityForCustomers),
                NumberOfCustomerParking = query.Sum(c => c.CurrnetCapacityForCustomers),
                NumberOfEmployeeEmptyParking = query.Sum(c => c.MaxSlotsForEmployess - c.CurrnetCapacityEmployees),
                NumberOfEmployeeParking = query.Sum(c => c.CurrnetCapacityEmployees),
                NumberOfVisitorParking = query.Sum(c => c.CurrnetCapacityForFree),
                NumberOfVisitorEmptyParking = query.Sum(c => c.MaxSlotsForFree - c.CurrnetCapacityForFree),
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
        var result= 
            await query
            .Where(payment => payment.CreationTime >= oneWeekAgo.Date)
            .GroupBy(payment => new { payment.CreationTime.Date , payment.PaymentCategory })
            .Select(group => new PaymentGroup
            {
                Date = group.Key.Date,
                PaymentCategory = group.Key.PaymentCategory,
                DayOfWeek = group.Key.Date.DayOfWeek,
                TotalAmount = group.Sum(payment => (double) payment.Amount)
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
                    AllTotalAmount = paymentGroup.Sum(p => p.TotalAmount ),
                    InvoiceTotalAmount = paymentGroup.Where(p => p.PaymentCategory == PaymentCategory.PaymentParking).Sum(p => p.TotalAmount ),
                    PackageTotalAmount = paymentGroup.Where(p => p.PaymentCategory == PaymentCategory.Package).Sum(p => p.TotalAmount )
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

    public async Task<(string? Day,double? totalAmounts)> CalculateDailyCollectedMoneyMdKhlAsync()
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
    public async Task<(string? Day,double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync()
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
        query =  query.Where(payment => payment.CreationTime.DayOfYear == now.DayOfYear);
        var result =  await query.FirstOrDefaultAsync();
        var finalResult = new FinalResult
        {

            DayOfWeek = result?.CreationTime.ToString("dddd", culture),
            AllTotalAmount = query.Sum(payment => (double)payment.Amount)
        };
        return (finalResult?.DayOfWeek?.ToString(), finalResult?.AllTotalAmount);
    }

    public async Task<(string? Day,double? totalAmounts)> CalculateDailyCollectedMoneyOrganizationAsync(int organizationId)
    {
        var query = await _paymentRepository.GetQueryableAsync();
        query = query.Where(_ => _.OrganizationId == organizationId);
        return await GetCollectMoneyToday(query);
    }

    public async Task<TableData> ReortGarageTransactionMdkhlAsync(CustomerCategoryStatus category)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysis(category);
            var dataTable = CreateTableHelper.CreateDataTableForGarageTransactionCustomer(result);
            return dataTable;
        }
        
    }

    public async Task<TableData> ReortGarageTransactionPlateAsync(string PlateNumber)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisPlate(PlateNumber);
            var dataTable = CreateTableHelper.CreateDataTableForGarageTransactionCustomer(result);
            return dataTable;
        }

    }
    public async Task<TableData> ReortGarageTransactionCustomerIdAsync(Guid CustomerId)
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysisCustomerId(CustomerId);
            var dataTable = CreateTableHelper.CreateDataTableForGarageTransactionCustomer(result);
            return dataTable;
        }

    }

    public async Task<TableData> ReortGarageTransactionOrganizationAsync(CustomerCategoryStatus category)
    {
        var result = await _garageTransactionAnalysis.GetGarageTransactionsAnalysis(category);
        var dataTable = CreateTableHelper.CreateDataTableForGarageTransaction(result);
        return dataTable;
    }

    public async Task<TableData> ReortPaymentCollectedMoneyDailyAndMonthlyOrganizationAsync()
    {
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        var result = await _paymentAnalysisRepository.GetPaymentCollectedMoneyDailyAndMonthlyAnalysis(organization.Id);
        var dataTable = CreateTableHelper.CreateDataTableForPaymentCollectedMoneyDailyAndMonthly(result);
        return dataTable;
    }

    public async Task<TableData> ReortPaymentCollectedMoneyDailyAndMonthlyMdkhlAsync()
    {
            var result = await _paymentAnalysisRepository.GetPaymentCollectedMoneyDailyAndMonthlyAnalysis(null);
            var dataTable = CreateTableHelper.CreateDataTableForPaymentCollectedMoneyDailyAndMonthly(result);
            return dataTable;
        
    }

    public async Task<(int totalCountOfCustomers,int totalCountOfVisitors,int totalCountOfEmployees,int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesMdkhl()
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
                return (0, 0, 0,0);
            }
            return (groupedCounts.CountOfCustomers, groupedCounts.CountOfVisitors, groupedCounts.CountOfEmployees, groupedCounts.CountOfNoPackage);
        }
       
    }
    
    public async Task<(int totalCountOfCustomers, int totalCountOfVisitors, int totalCountOfEmployees , int totalCountOfNoPackage)> GetCountOfCustomersAndVisitorsAndEmployeesOrganization(int organizationId)
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
            return (0, 0, 0,0);
        }
        return (groupedCounts.CountOfCustomers, groupedCounts.CountOfVisitors, groupedCounts.CountOfEmployees,groupedCounts.CountOfNoPackage);
    }
}
