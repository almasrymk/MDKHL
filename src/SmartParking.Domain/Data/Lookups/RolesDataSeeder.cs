using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace SmartParking.Data.Lookups;

public class RolesDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;

    public RolesDataSeeder(IIdentityRoleRepository roleRepository, IdentityRoleManager roleManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Check if the role already exists
        if (await _roleRepository.FindByNormalizedNameAsync(RolesType.Mdkhl.ToString().ToUpper()) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), RolesType.Mdkhl.ToString()));
        }

        if (await _roleRepository.FindByNormalizedNameAsync(RolesType.Organization.ToString().ToUpper()) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), RolesType.Organization.ToString()));
        }
        if (await _roleRepository.FindByNormalizedNameAsync(RolesType.Customer.ToString().ToUpper()) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), RolesType.Customer.ToString()));
        }        
        if (await _roleRepository.FindByNormalizedNameAsync(RolesType.MdkhlEmployee.ToString().ToUpper()) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), RolesType.MdkhlEmployee.ToString()));
        }        
        if (await _roleRepository.FindByNormalizedNameAsync(RolesType.OrganizationEmployee.ToString().ToUpper()) == null)
        {
            await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), RolesType.OrganizationEmployee.ToString()));
        }
        

        // Add more roles as needed
    }
}
