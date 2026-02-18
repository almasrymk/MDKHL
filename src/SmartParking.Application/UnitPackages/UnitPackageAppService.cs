
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.UnitPackages;

public class UnitPackageAppService : CrudAppService<UnitPackage, UnitPackageDto, int, PagedAndSortedResultRequestDto, CreateUpdateUnitPackageDto>, 
    IUnitPackageAppService
{
    private readonly IRepository<UnitPackage, int> _repository;
    private readonly IDataFilter _dataFilter;

    public UnitPackageAppService(IRepository<UnitPackage, int> repository, 
        IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    public override async Task<PagedResultDto<UnitPackageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var unitPackage = await base.GetListAsync(input);
            return unitPackage;
        }
    }

    public override async Task<UnitPackageDto> CreateAsync(CreateUpdateUnitPackageDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            await CheckExistName(input);
            return await base.CreateAsync(input);
        }
    }

    public override async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var package = await _repository.FindAsync(id);

            if (package is null)
            {
                throw new EntityNotFoundException(typeof(VehicleModel), id);
            }

            await _repository.HardDeleteAsync(package);
        }
    }

    public override async Task<UnitPackageDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }

    public async Task<PagedResultDto<UnitPackageDto>> GetUnitPackagesByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();

            query = query.Where(x => x.Package.OrganizationId == organizationId);

            var totalCount = await AsyncExecuter.CountAsync(query);

            query = query.OrderBy(x => x.Package.EnglishName)
                         .Skip(input.SkipCount)
                         .Take(input.MaxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);

            var garagesDto = entities.Select(x => {
                var dto = ObjectMapper.Map<UnitPackage, UnitPackageDto>(x);
                dto.Package = ObjectMapper.Map<Package, PackageDto>(x.Package);
                return dto;
            }).ToList();

            return new PagedResultDto<UnitPackageDto>(totalCount, garagesDto);
        }
    }

    public override async Task<UnitPackageDto> UpdateAsync(int id, CreateUpdateUnitPackageDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var dto = await _repository.GetAsync(id);
            if (dto.Package.Status == Status.Active && input.Package.Status == Status.Inactive)
            {
                input.Package.DeactivationDate = DateTime.Now;
            }
            else if (dto.Package.Status == Status.Inactive && input.Package.Status == Status.Active && dto.Package.PackageCategoryType != PackageCategoryTypes.CustomPackage)
            {
                input.Package.ActivationDate = DateTime.Now;
            }
            await CheckExistNameForUpdate(id, input);
            return await base.UpdateAsync(id, input);
        }
    }

    #region Private methods
    private async Task CheckExistName(CreateUpdateUnitPackageDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var packageEn = await query.Where(x => x.Package.EnglishName == input.Package.EnglishName || x.Package.ArabicName == input.Package.ArabicName)
            .FirstOrDefaultAsync();
        if (packageEn != null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("DuplicationPackageName"));
        }
    }

    private async Task CheckExistNameForUpdate(int Id, CreateUpdateUnitPackageDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var packageEn = await query.Where(x => (x.Package.EnglishName == input.Package.EnglishName || x.Package.ArabicName == input.Package.ArabicName) && x.Id != Id)
            .FirstOrDefaultAsync();
        if (packageEn != null)
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("DuplicationPackageName"));
        }
    }

    public async Task<List<UnitPackageDto>> GetUnitPackagesByCategory(CustomerCategoryStatus customerCategory)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.Package.CustomerCategory == customerCategory && x.Package.Status == Status.Active);
        var entities = await AsyncExecuter.ToListAsync(query);
        var unitPackageDtos = entities.Select(x => {
            var dto = ObjectMapper.Map<UnitPackage, UnitPackageDto>(x);
            dto.Package = ObjectMapper.Map<Package, PackageDto>(x.Package);
            return dto;
        }).ToList();
        return unitPackageDtos;
        
    }

    public async Task<List<UnitPackageDto>> GetUnitPackagesByCategory(int organizationId, CustomerCategoryStatus customerCategory)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(x => x.Package.CustomerCategory == customerCategory && x.Package.Status == Status.Active && x.Package.OrganizationId==organizationId );
            var entities = await AsyncExecuter.ToListAsync(query);
            var unitPackageDtos = entities.Select(x => {
                var dto = ObjectMapper.Map<UnitPackage, UnitPackageDto>(x);
                dto.Package = ObjectMapper.Map<Package, PackageDto>(x.Package);
                return dto;
            }).ToList();
            return unitPackageDtos;
        }

    }
    #endregion
}
