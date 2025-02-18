using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParking.DailyShifts;
public class DailyShiftAppService :
      CrudAppService<
        DailyShift,
        DailyShiftDto,
        int,
        PagedAndSortedAndSearchResultRequestDto,
        CreateUpdateDailyShiftDto
        >,
    IDailyShiftAppService
{
    private readonly IRepository<DailyShift, int> _repository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IMyUserRepository _userRepository;
    private readonly IRepository<Organization, int> _organizationRepository;

    private IDataFilter _dataFilter;

    public DailyShiftAppService(IRepository<DailyShift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository ,
        IRepository<Garage, int> garageRepository, IMyUserRepository userRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageRepository = garageRepository;
        _userRepository = userRepository;
    }
    public override async Task<DailyShiftDto> CreateAsync(CreateUpdateDailyShiftDto input)
    {
        if (!input.TenantId.HasValue)
            input.TenantId = CurrentTenant.Id;
        return await base.CreateAsync(input);
    }
    public override async Task<DailyShiftDto> UpdateAsync(int id, CreateUpdateDailyShiftDto input)
    {
        return await base.UpdateAsync(id, input);
    }
    public override  Task DeleteAsync(int id)
    {
        return  base.DeleteAsync(id);
    }
    public override async Task<DailyShiftDto?> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var IsFounddailyShift = await _repository.FirstOrDefaultAsync(_ => _.Id == id);
            if (IsFounddailyShift is null)
                throw new Exception("NotFoundValidation"); 
            var result = from dailyShift in queryDailyShift
                         where dailyShift.Id == id
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join organization in queryOrganization
                         on dailyShift.OrganizationId equals organization.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id

                         select new DailyShiftDto
                         {
                             Id = dailyShift.Id,
                             KeeperId = dailyShift.KeeperId,
                             OrganizationId = dailyShift.OrganizationId,
                             OrganizationNameAr = organization.ArabicName,
                             OrganizationNameEn = organization.EnglishName,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             RequestTime = dailyShift.RequestTime,
                             KeeperShiftRequestType = dailyShift.KeeperShiftRequestType,
                             KeeperName = user.Name,
                         };
            return await result.FirstOrDefaultAsync();
        }
        
    }
    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsAsync (PagedAndSortedAndSearchResultRequestDto input ,Guid?  TenantId )
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();

            var result = from dailyShift in queryDailyShift
                         where TenantId.HasValue ? dailyShift.TenantId == TenantId : true
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join organization in queryOrganization
                         on dailyShift.OrganizationId equals organization.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id
                         
                         select new DailyShiftDto
                         {
                             Id = dailyShift.Id,
                             KeeperId = dailyShift.KeeperId,
                             OrganizationId = dailyShift.OrganizationId,
                             OrganizationNameAr = organization.ArabicName,
                             OrganizationNameEn = organization.EnglishName,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             RequestTime = dailyShift.RequestTime,
                             KeeperShiftRequestType = dailyShift.KeeperShiftRequestType,
                             KeeperName = user.Name,
                         };
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            result = ApplySort(input, result);
            result = ApplySearch(input, result);
            var total = result.Count();
            return new PagedResultDto<DailyShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }

    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? TenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();

            var result = from dailyShift in queryDailyShift
                         where TenantId.HasValue ? dailyShift.TenantId == TenantId : true
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id
                         select new DailyShiftDto
                         {
                             Id = dailyShift.Id,
                             KeeperId = dailyShift.KeeperId,
                             OrganizationId = dailyShift.OrganizationId,
                             OrganizationNameAr = string.Empty,
                             OrganizationNameEn = string.Empty,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             RequestTime = dailyShift.RequestTime,
                             KeeperShiftRequestType = dailyShift.KeeperShiftRequestType,
                             KeeperName = user.Name,
                         };
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            result = ApplySort(input, result);
            result = ApplySearch(input, result);
            var total = result.Count();
            return new PagedResultDto<DailyShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }
    private static IQueryable<DailyShiftDto> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DailyShiftDto> result)
    {
        var sorting = input.Sorting;
        if (sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        result = result.OrderBy(sorting);
        return result;
    }

    private static IQueryable<DailyShiftDto> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DailyShiftDto> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.KeeperName.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.GarageNameAr.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.GarageNameEn.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.OrganizationNameAr.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.OrganizationNameEn.ToLower().Contains(input.SearchingTerm.ToLower())  ||
                or.KeeperShiftRequestType.ToString().Contains(input.SearchingTerm.ToLower())  ||
                or.RequestTime.ToString().Contains(input.SearchingTerm)


            );
        }

        return queryable;
    }
}
