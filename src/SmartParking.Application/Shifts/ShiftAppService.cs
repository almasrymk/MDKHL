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
using SmartParking.Enums;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
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
        var _overlapExpression = GetTimeOverlapExpression(input.StartTime, input.EndTime , input.StartDate, input.EndDate, input.GarageId,input.IsFullDay,input.Status);
        if ( await HasOverlappingShift(_overlapExpression))
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
        var result = await repository.FirstOrDefaultAsync(expression);
        Console.WriteLine($"{result?.EnglishName}  {result?.StartDate} {result?.EndDate}  {result?.StartTime} {result?.EndTime} ");
        return await repository.AnyAsync(expression);
    }
    public Expression<Func<Shift, bool>> GetTimeOverlapExpression(TimeOnly StartTime, TimeOnly EndTime, DateOnly StartDate, DateOnly EndDate, int garageId, bool IsFullday, Status status)
    {
        // Base condition: Garage ID, date range, and status
        Expression<Func<Shift, bool>> baseCondition = s =>
            s.GarageId == garageId &&
            s.StartDate <= StartDate &&
            s.EndDate >= StartDate &&
            s.Status == Status.Active && status == Status.Active;

        // Case 1: Existing shift is a full-day shift
        Expression<Func<Shift, bool>> existingShiftIsFullDay = AnExistingShiftIsFullDay(StartDate);

        // Case 2: New shift is a full-day shift
        Expression<Func<Shift, bool>> currentShiftIsFullDay = CurrentShiftIsFullDay(IsFullday, StartDate);

        // Case 3a: No midnight shift (StartTime <= EndTime)
        Expression<Func<Shift, bool>> newShiftIsNotMidnight = NewShiftIsNotMidNightShift(StartTime, EndTime);

        // Case 3b: Existing shift is a midnight shift (StartTime > EndTime)
        Expression<Func<Shift, bool>> newShiftIsMidnight = NewShiftIsMidNightShift(StartTime, EndTime);

        if (IsFullday) { 
            var baseConditionAndExistingShiftFullDay = baseCondition.And(existingShiftIsFullDay);
            return baseConditionAndExistingShiftFullDay.And(currentShiftIsFullDay);
        }
        else
        {
            if (StartTime < EndTime)
            {
                 // Case 3a
                return baseCondition.And(newShiftIsNotMidnight);
            }
            else
            {
                // Case 3b
                return baseCondition.And(newShiftIsMidnight);
            }
        }
    }

    public Expression<Func<Shift, bool>> NotSameEntityExpression(int ShiftId)
    {
        return s => s.Id != ShiftId;
    }

    public Expression<Func<Shift, bool>> AnExistingShiftIsFullDay(DateOnly StartDate)
    {
        return s => s.IsFullDay && (
                       (StartDate <= s.EndDate && StartDate >= s.StartDate) // New shift overlaps with the full-day shift's date range
                   );
    }

    public Expression<Func<Shift, bool>> CurrentShiftIsFullDay(bool IsFullday, DateOnly StartDate)
    {
        return s => IsFullday && (
                       (s.StartDate <= StartDate && s.EndDate >= StartDate) // Existing shift overlaps with the new full-day shift's date range
                   );
    }

    public Expression<Func<Shift, bool>> NewShiftIsNotMidNightShift(TimeOnly StartTime, TimeOnly EndTime)
    {
        return s => s.StartTime <= s.EndTime && (
                           (StartTime >= s.StartTime && StartTime < s.EndTime) || // New shift starts during existing shift
                           (EndTime > s.StartTime && EndTime <= s.EndTime) ||    // New shift ends during existing shift
                           (StartTime <= s.StartTime && EndTime >= s.EndTime)    // New shift completely overlaps existing shift
                       );
    }

    public Expression<Func<Shift, bool>> NewShiftIsMidNightShift(TimeOnly StartTime, TimeOnly EndTime)
    {
        return s => s.StartTime > s.EndTime && (
                       // New shift ends before the existing shift starts
                       (EndTime <= s.StartTime) ||
                       // New shift starts after the existing shift ends
                       (StartTime >= s.EndTime) ||
                       // New shift overlaps with the existing shift
                       (StartTime < s.StartTime && EndTime > s.EndTime) || // New shift spans the entire existing shift
                       (StartTime >= s.StartTime && EndTime <= s.EndTime) || // New shift is entirely within the existing shift
                       (StartTime < s.StartTime && EndTime > s.StartTime) || // New shift starts before and ends during the existing shift
                       (StartTime < s.EndTime && EndTime > s.EndTime) // New shift starts during and ends after the existing shift
                     );
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
                             IsFullDay=Shifts.IsFullDay,
                             TenantId=Shifts.TenantId,
                             GarageId=Shifts.GarageId,
                             CreationTime=Shifts.CreationTime
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
                         TenantId = Shifts.TenantId,
                         GarageId = Shifts.GarageId,
                         CreationTime = Shifts.CreationTime,
                         IsFullDay=Shifts.IsFullDay,
                         
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
        var _overlapExpression  = GetTimeOverlapExpression(input.StartTime, input.EndTime, input.StartDate, input.EndDate, input.GarageId,input.IsFullDay,input.Status);
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
                        where Shifts.Id == id
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
                            GarageId = Shifts.GarageId,
                            GarageArabicName = Garages.NameAr,
                            GarageEnglishName = Garages.NameEn,
                            OrganizationArabicName = organization.ArabicName,
                            OrganizationEnglishName = organization.EnglishName,
                            TenantId = Shifts.TenantId,
                            LastModificationTime = Shifts.LastModificationTime,
                            LastModifierId = Shifts.LastModifierId,
                            CreationTime = Shifts.CreationTime,
                            CreatorId = Shifts.CreatorId,
                            IsFullDay=Shifts.IsFullDay
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

    public async Task<ShiftDto> UpdateMDKHLAsync(int id, CreateUpdateShiftDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await UpdateAsync(id,input);
        }
    }
}