using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
using System;
using System.Collections.Generic;
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

    public AnalysisAppService(IRepository<Customer, Guid> custeRepo,
        IRepository<Organization, int> orgRepo, IRepository<Invoice, int> invoiceRepo,
        IDataFilter datafilter,IRepository<Garage,int> GarageRepository,
        IRepository<Payment,Guid> paymentRepository)
    {
        _custeRepo = custeRepo;
        _orgRepo = orgRepo;
        _invoiceRepo = invoiceRepo;
        _datafilter = datafilter;
        _garageRepository = GarageRepository;
        _paymentRepository = paymentRepository;
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


    public async Task<(string[] Days , double[] )> CalculateWeeklyCollectedMoneyAsync()
    {

        DateTime now = DateTime.Now;
        DateTime oneWeekAgo = now.AddDays(-7);
        var query = await _paymentRepository.GetQueryableAsync();

        var allDays = Enumerable.Range(0, 7)
                        .Select(i => oneWeekAgo.AddDays(i).Date)
                        .ToList();

        query = query.Where(_ => _.CreationTime >= oneWeekAgo.Date && _.CreationTime <= now.Date);

        var result = await query
       .GroupBy(payment => payment.CreationTime.Date)
       .Select(group => new
       {
           Day = group.Key, 
           TotalAmount = group.Sum(payment => payment.Amount) 
       })
       .ToListAsync();
        var mergedResults = allDays.GroupJoin(
                result,
                day => day,
                r => r.Day,
                (day, matchingResults) => new
                {
                    Day = day.DayOfWeek.ToString(), // Get day of the week name
                    TotalAmount = matchingResults.Any()
                        ? (double)matchingResults.Sum(r => r.TotalAmount) // Convert decimal to double
                        : 0.0 // Default to 0 if no data for the day
                }
            ).ToList();

        // Prepare the result as two arrays
        string[] days = mergedResults.Select(r => r.Day).ToArray();
        double[] totalAmounts = mergedResults.Select(r => r.TotalAmount).ToArray();


        //var dailyCollection = new Dictionary<string[], double[]>();


        //var daysOfWeek = Enumerable.Range(0, 7)
        //    .Select(i => oneWeekAgo.AddDays(i))
        //    .ToArray();

        //foreach (var day in daysOfWeek)
        //{
        //    var dayName = day.ToString("dddd"); 
        //    var dayData = result.FirstOrDefault(r => r.Day == day.Date);
        //    double collectedAmount = dayData != null ? Convert.ToDouble(dayData.TotalAmount) : 0;

        //    dailyCollection[dayName] = new double[] { collectedAmount };
        //}

        //return dailyCollection;
        // Prepare the result as two arrays
        //string[] days = result.Select(r => r.Day.ToString("dddd")).ToArray();
        //double[] totalAmounts = result.Select(r => (double)r.TotalAmount).ToArray();

        // Return the tuple
        return (days, totalAmounts);
    }
}
