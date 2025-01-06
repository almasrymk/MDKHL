using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using SmartParking.DataTable;
using SmartParking.Entities.Analysis;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Helpers;
using SmartParking.Localization;
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
    private readonly IGarageTransactionAnalysisRepository _garageTransactionAnalysis;
    private readonly IPaymentAnalysisRepository _paymentAnalysisRepository;

    private class PaymentGroup
    {
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public double TotalAmount { get; set; }
    }

    private class FinalResult
    {
        public DateTime Day { get; set; }
        public string DayOfWeek { get; set; }
        public double TotalAmount { get; set; }
    }
    public AnalysisAppService(IRepository<Customer, Guid> custeRepo,
        IRepository<Organization, int> orgRepo, IRepository<Invoice, int> invoiceRepo,
        IDataFilter datafilter,IRepository<Garage,int> GarageRepository,
        IRepository<Payment,Guid> paymentRepository,
        IGarageTransactionAnalysisRepository garageTransactionAnalysis,
        IPaymentAnalysisRepository paymentAnalysisRepository)
    {
        _custeRepo = custeRepo;
        _orgRepo = orgRepo;
        _invoiceRepo = invoiceRepo;
        _datafilter = datafilter;
        _garageRepository = GarageRepository;
        _paymentRepository = paymentRepository;
        _garageTransactionAnalysis = garageTransactionAnalysis;
        _paymentAnalysisRepository = paymentAnalysisRepository;
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


    public async Task<(string[] Days, double[] TotalAmounts)> CalculateWeeklyCollectedMoneyMdkhlAsync()
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
    
    public async Task<(string[] Days, double[] TotalAmounts)> CalculateWeeklyCollectedMoneyOrganizationAsync(int organizationId)
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
        return await query
            .Where(payment => payment.CreationTime >= oneWeekAgo.Date)
            .GroupBy(payment => payment.CreationTime.Date)
            .Select(group => new PaymentGroup
            {
                Date = group.Key,
                DayOfWeek = group.Key.DayOfWeek,
                TotalAmount = group.Sum(payment => (double) payment.Amount)
            })
            .ToListAsync();
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

        return allDays
            .GroupJoin(
                payments,
                day => day,
                payment => payment.Date,
                (day, paymentGroup) => new FinalResult
                {
                    Day = day,
                    DayOfWeek = day.ToString("dddd"),
                    TotalAmount = paymentGroup.Sum(p => p.TotalAmount)
                })
            .GroupBy(d => d.DayOfWeek)
            .Select(g => g.OrderByDescending(d => d.Day).First())
            .OrderBy(d => ((int)d.Day.DayOfWeek - (int)firstDayOfWeek + 7) % 7)
            .ToList();
    }

    private (string[] days, double[] totalAmounts) PrepareResult(List<FinalResult> finalResult, CultureInfo culture)
    {
        string[] days = finalResult.Select(r => r.DayOfWeek).ToArray();
        double[] totalAmounts = finalResult.Select(r => (double)r.TotalAmount).ToArray();

        if (culture.Name == "ar")
        {
            Array.Reverse(days);
            Array.Reverse(totalAmounts);
        }

        return (days, totalAmounts);
    }

    public async Task<(string? Day,double? totalAmounts)> CalculateDailyCollectedMoneyMdKhlAsync()
    {
        
        var query = await _paymentRepository.GetQueryableAsync();
        
        return await GetCollectMoneyToday( query);
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
            TotalAmount = query.Sum(payment => (double)payment.Amount)
        };
        return (finalResult?.DayOfWeek?.ToString(), finalResult?.TotalAmount);
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
            var dataTable = CreateTableHelper.CreateDataTableForGarageTransaction(result);
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
        var result = await _paymentAnalysisRepository.GetPaymentCollectedMoneyDailyAndMonthlyAnalysis();
        var dataTable = CreateTableHelper.CreateDataTableForPaymentCollectedMoneyDailyAndMonthly(result);
        return dataTable;
    }

    public async Task<TableData> ReortPaymentCollectedMoneyDailyAndMonthlyMdkhlAsync()
    {
        using (_datafilter.Disable<IMultiTenant>())
        {
            var result = await _paymentAnalysisRepository.GetPaymentCollectedMoneyDailyAndMonthlyAnalysis();
            var dataTable = CreateTableHelper.CreateDataTableForPaymentCollectedMoneyDailyAndMonthly(result);
            return dataTable;
        }
    }
}
