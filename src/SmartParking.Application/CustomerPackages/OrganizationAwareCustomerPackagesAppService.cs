using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.Organizations;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages;

[ExposeServices(typeof(IAssignCustomerPackagesAppService))]
public class OrganizationAwareCustomerPackagesAppService : ApplicationService, IAssignCustomerPackagesAppService
{
    private readonly Func<NormalCustomerPackagesAppService> _normalFactory;
    private readonly Func<UnitCustomerPackageAppService> _unitFactory;
    private readonly IOrganizationsAppService _organizationAppService;

    public OrganizationAwareCustomerPackagesAppService(
        Func<NormalCustomerPackagesAppService> normalFactory,
        Func<UnitCustomerPackageAppService> unitFactory,
        IOrganizationsAppService organizationAppService)
    {
        _normalFactory = normalFactory;
        _unitFactory = unitFactory;
        _organizationAppService = organizationAppService;
    }

    public async Task<Result<CustomerPackageDto>> AssignPackageToCustomerAsync(CreateUpdateCustomerPackageDto input)
        => await (await ResolveAsync()).AssignPackageToCustomerAsync(input);

    public async Task<CustomerPackageDto> UpdateCustomerPackage(int customerPackageId, CreateUpdateCustomerPackageDto input)
        => await (await ResolveAsync()).UpdateCustomerPackage(customerPackageId, input);

    public async Task<Result<CustomerPackageDto>> UpdateDefaultCustomerPackage(CreateUpdateCustomerPackageDto input)
        => await (await ResolveAsync()).UpdateDefaultCustomerPackage(input);

    private async Task<IAssignCustomerPackagesAppService> ResolveAsync()
    {
        // TODO: confirm actual method name — placeholder below
        var organization = await _organizationAppService.GetOrganizationForCurrentTenatAsync();
        var settings = organization.OrganizationSettings; // adjust to actual property path on OrganizationDto

        return settings switch
        {
            { VehcileAccess: true, UserAccess: false } => _normalFactory(),
            { UserAccess: true, VehcileAccess: false } => _unitFactory(),
            _ => throw new BusinessException("CustomerPackage:AmbiguousOrganizationAccessConfiguration")
                .WithData("TenantId", organization.TenantId)
                .WithData("UserAccess", settings.UserAccess)
                .WithData("VehcileAccess", settings.VehcileAccess)
        };
    }
}