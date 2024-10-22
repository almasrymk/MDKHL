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


    public async Task<OrganizationCustomersDto> GetNumberOfCustomerForOrganizations()
    {

        using (_datafilter.Disable<IMultiTenant>())
        {
            var query = await _orgRepo.GetQueryableAsync();
            query=query.Include(x => x.Customers);
            var result= query.Select(x => new Organization { EnglishName = x.EnglishName }).ToList();
            var result2= query.Select(x => new Organization { Customers=x.Customers }).ToList();
            OrganizationCustomersDto organizationCustomersDto = new OrganizationCustomersDto();
            organizationCustomersDto.Names= ObjectMapper.Map<List<Organization>, List<string>>(result);
            organizationCustomersDto.CustomersCount= ObjectMapper.Map<List<Organization>, List<double>>(result2);
            return organizationCustomersDto;
        }
    


    }
}
