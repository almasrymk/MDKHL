using Abp.Application.Services;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Analysis;

public class AnalysisAppService : SmartParkingAppService, IAnalysisAppService
{
    private readonly IRepository<Invoice,int> _invoiceRepo;
    private readonly IRepository<Organization,int> _orgRepo;
    private readonly IRepository<Customer,Guid> _custeRepo;
    private readonly IRepository<CustomerOrganization,int> _custmrOrgRepo;
    private readonly IDataFilter _datafilter;

    public AnalysisAppService(IRepository<Customer, Guid> custeRepo, IRepository<Organization, int> orgRepo, IRepository<Invoice, int> invoiceRepo, IDataFilter datafilter)
    {
        _custeRepo = custeRepo;
        _orgRepo = orgRepo;
        _invoiceRepo = invoiceRepo;
        _datafilter = datafilter;
    }


    public async Task<AnalysisNamesCountDto> GetNumberOfCustomerForOrganizations()
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _orgRepo.GetQueryableAsync();
            query=query.Include(x => x.Customers);
            var result= query.Select(x => new Organization { EnglishName = x.EnglishName }).ToList();
            var result2= query.Select(x => new Organization { Customers=x.Customers }).ToList();
            AnalysisNamesCountDto namesCountDto = new AnalysisNamesCountDto();
            namesCountDto.Names= ObjectMapper.Map<List<Organization>, List<string>>(result);
            namesCountDto.Count= ObjectMapper.Map<List<Organization>, List<double>>(result2);
            return namesCountDto;
        }
    


    }

    public async Task<AnalysisNamesCountDto> GetPackageAndNumberOfOrganizationsThatSubscribed()
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query =await _orgRepo.GetQueryableAsync();
            query = query.Include(x => x.Subscription);
            query = query.OrderByDescending(org => org.SubscriptionId);
            var result = query.GroupBy(org => org.SubscriptionId)
                .Select(x =>(double) x.Sum(o=>o.Id) ).ToList();
            var result2=  query
                .Select(org=>org.Subscription.EnglishName).ToList();

            AnalysisNamesCountDto namesCountDto = new AnalysisNamesCountDto();
            namesCountDto.Names = result2;
            namesCountDto.Count = result;
            return namesCountDto;
        }
    }
}
