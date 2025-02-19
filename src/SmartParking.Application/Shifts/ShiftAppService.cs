using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq.Dynamic.Core;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;
using Abp.Localization;
using SmartParking.Localization;
using LocalizationHelper = SmartParking.Localization.LocalizationHelper;
using System.Linq.Expressions;
namespace SmartParking.Shifts;

[Authorize]

public class ShiftAppService : CrudAppService<Shift, ShiftDto, int, PagedAndSortedResultRequestDto, CreateUpdateShiftDto>, IShiftAppService
{
    private readonly IRepository<Shift, int> repository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private IDataFilter _dataFilter;



    public ShiftAppService(IRepository<Shift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository, IRepository<Garage, int> garageRepository) : base(repository)
    {
        this.repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageRepository = garageRepository;
    }


    public override async Task<ShiftDto> CreateAsync(CreateUpdateShiftDto input)
    {
        await CheckExistanceOfDependencies(input);
        var _overlapExpression = GetTimeOverlapExpression(input.StartTime, input.EndTime, input.GarageId);
        if (await HasOverlappingShift(_overlapExpression))
        {
            throw new Exception("Shift overlaps with an existing shift.");
        }
        return await base.CreateAsync(input);
    }

    private async Task CheckExistanceOfDependencies(CreateUpdateShiftDto input)
    {
        if (!await CheckGarageExists(input.GarageId))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Garage") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        if (!await CheckOrganizationExists(input.OrganizationId))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("Organization") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
    }

    public async Task<bool> CheckGarageExists(int GarageId)
    {
        var garage = await _garageRepository.FindAsync(GarageId);
        if (garage == null)
        {
            return false;
        }
        return true;

    }
    public async Task<bool> CheckOrganizationExists(int OrganizationId)
    {
        var organization = await _organizationRepository.FindAsync(OrganizationId);
        if (organization == null)
        {
            return false;
        }
        return true;

    }
    public async Task<bool> HasOverlappingShift(Expression<Func<Shift, bool>> expression)
    {
        return await repository.AnyAsync(expression);
    }
    public Expression<Func<Shift, bool>> GetTimeOverlapExpression(TimeOnly startTime, TimeOnly endTime, int garageId)
    {
        return s =>
             s.GarageId == garageId &&
            (startTime >= s.StartTime && startTime < s.EndTime) ||
            (endTime > s.StartTime && endTime <= s.EndTime) ||
            (startTime <= s.StartTime && endTime >= s.EndTime);
    }
    public Expression<Func<Shift, bool>> NotSameEntityExpression(int ShiftId)
    {
        return s => s.Id != ShiftId;
    }


    public async Task<PagedResultDto<ShiftDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {

        var queryable = await repository.GetQueryableAsync();
        var filterExpression = BuildExpression<Shift>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var shifts = await AsyncExecuter.ToListAsync(queryable);

        var shiftsDto = ObjectMapper.Map<List<Shift>, List<ShiftDto>>(shifts);
        return new PagedResultDto<ShiftDto>(
            totalCount,
            shiftsDto
        );
    }

    public Task<PagedResultDto<ShiftDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return Filter(input);
        }
    }

    public async Task<PagedResultDto<ShiftDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var _shiftQueryable = await repository.GetQueryableAsync();
            var _garageQueryable = await _garageRepository.GetQueryableAsync();
            var _organizationQueryable = await _organizationRepository.GetQueryableAsync();
            var shifts = from Shifts in _shiftQueryable
                         join Garages in _garageQueryable
                         on Shifts.GarageId equals Garages.Id
                         join organization in _organizationQueryable
                         on Shifts.OrganizationId equals organization.Id
                         where tenantId.HasValue ? organization.TenantId == tenantId : true
                         select new ShiftDto
                         {
                             Id = Shifts.Id,
                             ArabicName = Shifts.ArabicName,
                             EnglishName = Shifts.EnglishName,
                             Description = Shifts.Description,
                             StartDate = Shifts.StartDate,
                             EndDate = Shifts.EndDate,
                             StartTime = Shifts.StartTime,
                             EndTime = Shifts.EndTime,
                             Status = Shifts.Status,
                             OrganizationId = Shifts.OrganizationId,
                             GarageArabicName = Garages.NameAr,
                             GarageEnglishName = Garages.NameEn,
                             OrganizationArabicName = organization.ArabicName,
                             OrganizationEnglishName = organization.EnglishName,
                         };
            var query = ApplySearchMDKHL(input, shifts);
            return ProcessingOnShiftsQuery(input, shifts);
        }
    }

    private PagedResultDto<ShiftDto> ProcessingOnShiftsQuery(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> shifts)
    {
        var total = shifts.Count();
        shifts = ApplySort(input, shifts);
        shifts = shifts.Skip(input.SkipCount).Take(input.MaxResultCount);
        var result = shifts.ToList();
        return new PagedResultDto<ShiftDto>(
            total,
            result
        );
    }

    public async Task<PagedResultDto<ShiftDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var _shiftQueryable = await repository.GetQueryableAsync();
        var _garageQueryable = await _garageRepository.GetQueryableAsync();


        var shifts = from Shifts in _shiftQueryable
                     join Garages in _garageQueryable
                     on Shifts.GarageId equals Garages.Id


                     select new ShiftDto
                     {
                         Id = Shifts.Id,
                         ArabicName = Shifts.ArabicName,
                         EnglishName = Shifts.EnglishName,
                         Description = Shifts.Description,
                         StartDate = Shifts.StartDate,
                         EndDate = Shifts.EndDate,
                         StartTime = Shifts.StartTime,
                         EndTime = Shifts.EndTime,
                         Status = Shifts.Status,
                         OrganizationId = Shifts.OrganizationId,
                         GarageArabicName = Garages.NameAr,
                         GarageEnglishName = Garages.NameEn,
                     };
        ApplySearchOrganization(input, ref shifts);
        return ProcessingOnShiftsQuery(input, shifts);


    }

    private IQueryable<ShiftDto> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> queryable)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }

        return queryable;
    }

    private void ApplySearchOrganization(PagedAndSortedAndSearchResultRequestDto input, ref IQueryable<ShiftDto> queryable)
    {
        if (!input.SearchingTerm.IsNullOrEmpty())
        {
            var searchingItem = input.SearchingTerm.ToLower();
            queryable = queryable.Where(
                shift => shift.ArabicName.ToLower().Contains(searchingItem) ||
                shift.EnglishName.ToLower().Contains(searchingItem) ||
                shift.Description.ToLower().Contains(searchingItem) ||
                shift.StartDate.ToLongDateString().ToLower().Contains(searchingItem) ||
                shift.EndDate.ToLongDateString().ToLower().Contains(searchingItem) ||
                shift.GarageArabicName.ToLower().Contains(searchingItem) ||
                shift.GarageArabicName.ToLower().Contains(searchingItem)
                );

        }

    }
    private IQueryable<ShiftDto> ApplySearchMDKHL(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> queryable)
    {
        if (!input.SearchingTerm.IsNullOrEmpty())
        {
            queryable = queryable.Where(
              shift => shift.OrganizationArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
              shift.OrganizationEnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
            ApplySearchOrganization(input, ref queryable);
        }

        return queryable;
    }
    public async override Task<ShiftDto> UpdateAsync(int id, CreateUpdateShiftDto input)
    {
        await CheckExistanceOfDependencies(input);
        var _overlapExpression = GetTimeOverlapExpression(input.StartTime, input.EndTime, input.GarageId);
        var _sameShiftExpression = NotSameEntityExpression(id);
        if (await HasOverlappingShift(_overlapExpression.And(_sameShiftExpression)))
        {
            throw new Exception("Shift overlaps with an existing shift.");
        }
        return await base.UpdateAsync(id, input);
    }
    public async Task<ShiftDto> GetShiftAsync(int id, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var _shiftQueryable = await repository.GetQueryableAsync();
            var _garageQueryable = await _garageRepository.GetQueryableAsync();
            var _organizationQueryable = await _organizationRepository.GetQueryableAsync();
            var shift = from Shifts in _shiftQueryable
                        join Garages in _garageQueryable
                        on Shifts.GarageId equals Garages.Id
                        join organization in _organizationQueryable
                        on Shifts.OrganizationId equals organization.Id
                        where tenantId.HasValue ? organization.TenantId == tenantId : true && Shifts.Id == id

                        select new ShiftDto
                        {
                            Id = Shifts.Id,
                            ArabicName = Shifts.ArabicName,
                            EnglishName = Shifts.EnglishName,
                            Description = Shifts.Description,
                            StartDate = Shifts.StartDate,
                            EndDate = Shifts.EndDate,
                            StartTime = Shifts.StartTime,
                            EndTime = Shifts.EndTime,
                            Status = Shifts.Status,
                            OrganizationId = Shifts.OrganizationId,
                            GarageArabicName = Garages.NameAr,
                            GarageEnglishName = Garages.NameEn,
                            OrganizationArabicName = organization.ArabicName,
                            OrganizationEnglishName = organization.EnglishName,
                        };
            var shiftDto = shift.FirstOrDefault();
            if (shiftDto == null)
            {
                throw new Exception("NotFoundValidation");
            }
            return shiftDto;

        }
    }

    public async Task<ShiftDto> CreateForMDKHLAsync(CreateUpdateShiftDto shift)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
          return await  CreateAsync(shift);
        }
    }
}