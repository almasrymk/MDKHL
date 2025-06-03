using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using System.Linq.Dynamic.Core;
using LocalizationHelper = SmartParking.Localization.LocalizationHelper;
using System.Linq.Expressions;
using SmartParking.Enums;
using Volo.Abp;
using SmartParking.Filters;
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;

namespace SmartParking.Shifts;

[Authorize]

public class ShiftAppService : CrudAppService<Shift, ShiftDto, int, PagedAndSortedResultRequestDto, CreateUpdateShiftDto>, IShiftAppService
{
    private readonly IRepository<Shift, int> _repository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<KeeperShift, int> _shiftUserRepository;
    private IDataFilter _dataFilter;



    public ShiftAppService(IRepository<Shift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository,
        IRepository<Garage, int> garageRepository, IRepository<KeeperShift, int> shiftUserRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageRepository = garageRepository;
        _shiftUserRepository = shiftUserRepository;
    }
    #region public methods
    /// <inheritdoc/>
    public override async Task<ShiftDto> CreateAsync(CreateUpdateShiftDto input)
    {
        await CheckExistanceOfDependencies(input);
        var _overlapExpression = GetTimeOverlapExpression(input.StartTime, input.EndTime , input.StartDate, input.EndDate, input.GarageId,input.IsFullDay,input.Status);
        if ( await HasOverlappingShift(_overlapExpression) && (input.Status == Status.Active))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("ShiftOverlapValidation"));
        }
        return await base.CreateAsync(input);
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var _shiftQueryable = await _repository.GetQueryableAsync();
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
                             IsFullDay = Shifts.IsFullDay,
                             TenantId = Shifts.TenantId,
                             GarageId = Shifts.GarageId,
                             CreationTime = Shifts.CreationTime
                         };

            if (CurrentTenant.Id == null)
                shifts = ApplySearchMDKHL(input, shifts);
            else
                shifts = ApplySearchOrganization(input, shifts);
            return ProcessingOnShiftsQuery(input, shifts);
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<ShiftDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var _shiftQueryable = await _repository.GetQueryableAsync();
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
                         IsFullDay = Shifts.IsFullDay,
                     };
        var query = ApplySearchOrganization(input, shifts);
        return ProcessingOnShiftsQuery(input, query);
    }
    /// <inheritdoc/>
    public async override Task<ShiftDto> UpdateAsync(int id, CreateUpdateShiftDto input)
    {
        var Shift = await _repository.FirstOrDefaultAsync(_ => _.Id == id);
        await CheckExistanceOfDependencies(input);
        var _overlapExpression = GetTimeOverlapExpression(input.StartTime, input.EndTime, input.StartDate, input.EndDate, input.GarageId, input.IsFullDay, input.Status);
        var _sameShiftExpression = NotSameEntityExpression(id);
        if ((await HasOverlappingShift(_overlapExpression.And(_sameShiftExpression)))  && (input.Status == Status.Active))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("ShiftOverlapValidation"));
        }
        return await base.UpdateAsync(id, input);
    }
    /// <inheritdoc/>
    public async Task<ShiftDto> GetShiftAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var _shiftQueryable = await _repository.GetQueryableAsync();
            var _garageQueryable = await _garageRepository.GetQueryableAsync();
            var _organizationQueryable = await _organizationRepository.GetQueryableAsync();
            var shift = from Shifts in _shiftQueryable
                        where Shifts.Id == id
                        join Garages in _garageQueryable
                        on Shifts.GarageId equals Garages.Id
                        join organization in _organizationQueryable
                        on Shifts.OrganizationId equals organization.Id

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
                            IsFullDay = Shifts.IsFullDay
                        };
            var shiftDto = shift.FirstOrDefault();
            if (shiftDto == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return shiftDto;

        }
    }
    /// <inheritdoc/>
    public async Task<ShiftDto> CreateForMDKHLAsync(CreateUpdateShiftDto shift)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await CreateAsync(shift);
        }
    }
    /// <inheritdoc/>
    public async Task<ShiftDto> UpdateMDKHLAsync(int id, CreateUpdateShiftDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await UpdateAsync(id, input);
        }
    }
    public override async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            Shift? shift = await IsFoundShift(id);
            await CheckCountOfUsersAssignedToShift(id);
            await _repository.HardDeleteAsync(shift);
        }
    }
    #endregion
    #region Private Methods
    /// <summary>
    /// Applies sorting to the provided IQueryable of ShiftDto based on the input.
    /// </summary>
    /// <param name="input">The input DTO containing sorting information.</param>
    /// <param name="queryable">The IQueryable of ShiftDto to sort.</param>
    /// <returns>A sorted IQueryable of ShiftDto.</returns>
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
    /// <summary>
    /// Processes the IQueryable of ShiftDto for pagination and sorting.
    /// </summary>
    /// <param name="input">The input DTO containing pagination and sorting information.</param>
    /// <param name="shifts">The IQueryable of ShiftDto to process.</param>
    /// <returns>A paged result containing ShiftDto entries.</returns>
    private PagedResultDto<ShiftDto> ProcessingOnShiftsQuery(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> shifts)
    {
        var total = shifts.Count();
        shifts = ApplySort(input, shifts);
        shifts = shifts.Skip(input.SkipCount).Take(input.MaxResultCount);
        var result = shifts.ToList();
        return new PagedResultDto<ShiftDto>(total, result);
    }
    /// <summary>
    /// Checks whether a garage with the given ID exists.
    /// </summary>
    /// <param name="GarageId">The ID of the garage to check.</param>
    /// <returns>True if the garage exists, otherwise false.</returns>
    private async Task<bool> CheckGarageExists(int GarageId)
    {
        var garage = await _garageRepository.FindAsync(GarageId);
        return garage != null;
    }
    /// <summary>
    /// Checks whether an organization with the given ID exists.
    /// </summary>
    /// <param name="OrganizationId">The ID of the organization to check.</param>
    /// <returns>True if the organization exists, otherwise false.</returns>
    private async Task<bool> CheckOrganizationExists(int OrganizationId)
    {
        var organization = await _organizationRepository.FindAsync(OrganizationId);
        return organization != null;
    }
    /// <summary>
    /// Checks if there is any active overlapping shift based on the provided expression.
    /// </summary>
    /// <param name="expression">The filter expression to check for overlapping shifts.</param>
    /// <returns>True if an overlapping active shift exists, otherwise false.</returns>
    private async Task<bool> HasOverlappingShift(Expression<Func<Shift, bool>> expression)
    {
        var result= await _repository.FirstOrDefaultAsync(expression);
        return await _repository.AnyAsync(expression);
    }
    /// <summary>
    /// Returns an expression that excludes the shift with the given ID.
    /// </summary>
    /// <param name="ShiftId">The ID of the shift to exclude.</param>
    /// <returns>An expression that excludes the specified shift.</returns>
    private Expression<Func<Shift, bool>> NotSameEntityExpression(int ShiftId)
    {
        return s => s.Id != ShiftId;
    }
    /// <summary>
    /// Constructs a complex expression to detect time overlap between a new shift and existing ones.
    /// </summary>
    /// <param name="StartTime">The start time of the new shift.</param>
    /// <param name="EndTime">The end time of the new shift.</param>
    /// <param name="StartDate">The start date of the new shift.</param>
    /// <param name="EndDate">The end date of the new shift.</param>
    /// <param name="garageId">The garage ID to filter shifts.</param>
    /// <param name="IsFullday">Whether the new shift is a full-day shift.</param>
    /// <param name="status">The status of shifts to compare with.</param>
    /// <returns>An expression used to find overlapping shifts.</returns>
    private Expression<Func<Shift, bool>> GetTimeOverlapExpression(
        TimeOnly StartTime, TimeOnly EndTime, DateOnly StartDate, DateOnly EndDate, int garageId, bool IsFullday, Status status)
    {
        Expression<Func<Shift, bool>> baseCondition = BaseConditionForCreatingShift(garageId, StartDate, EndDate, status);
        Expression<Func<Shift, bool>> existingShiftIsFullDay = AnExistingShiftIsFullDayOverlapsWithNewShift(StartDate, EndDate);
        Expression<Func<Shift, bool>> overlapingWithMidnigtShift = IfNewNotOvernightOverlapsWithExistingMidnight(StartDate, EndDate, StartTime, EndTime);
        Expression<Func<Shift, bool>> middnighOverlapsWithMidnigtShift = IfNewOvernightOverlapsWithExistingMidnight(StartDate, EndDate, StartTime, EndTime);
        Expression<Func<Shift, bool>> currentShiftIsFullDay = CurrentShiftIsFullDayOverlapsWithExistingNotFullDayShift(IsFullday, StartDate, EndDate);
        Expression<Func<Shift, bool>> currentShiftIsFullDayOverlaping = NewShiftFullDayOverlapsWithExistingShiftNotFullDay(StartDate, EndDate);
        Expression<Func<Shift, bool>> newShiftIsNotMidnight = NewShiftIsNotMidNightShift(StartTime, EndTime,StartDate,EndDate);
        Expression<Func<Shift, bool>> newShiftIsMidnight = NewShiftIsMidNightShift(StartTime, EndTime,StartDate,EndDate);

        if (IsFullday)
        {
            return baseCondition.And(existingShiftIsFullDay)
                .Or(baseCondition.And(currentShiftIsFullDay))
                .Or(baseCondition.And(currentShiftIsFullDayOverlaping));
        }
        else
        {
            if (StartTime < EndTime)
            {
                return baseCondition.And(newShiftIsNotMidnight)
                    .Or(baseCondition.And(existingShiftIsFullDay))
                    .Or(baseCondition.And(overlapingWithMidnigtShift));
            }
            else
            {
                return baseCondition.And(newShiftIsMidnight)
                    .Or(baseCondition.And(middnighOverlapsWithMidnigtShift));
            }
        }
    }
    /// <summary>
    /// Constructs a base filter to find shifts that overlap with the given date range and belong to the specified garage and status.
    /// </summary>
    /// <param name="GarageId">Garage identifier.</param>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <param name="status">Status of the shift to match (typically Active).</param>
    /// <returns>An expression to match overlapping shifts by date and status.</returns>
    private Expression<Func<Shift, bool>> BaseConditionForCreatingShift(int GarageId, DateOnly StartDate, DateOnly EndDate, Status status)
    {
        return s =>
            s.GarageId == GarageId &&
            (s.Status == Status.Active);
    }
    private Expression<Func<Shift, bool>> OverlappingTimeNormalShiftExpression(TimeOnly StartTime, TimeOnly EndTime)
    {
        return s =>(s.StartTime>StartTime && StartTime < s.EndTime && (EndTime>s.EndTime||EndTime<s.EndTime))||
                   (s.StartTime<StartTime && StartTime < s.EndTime && (EndTime > s.EndTime||EndTime < s.EndTime));
    }
    private Expression<Func<Shift, bool>> OverlappingTimeMidnightShiftExpression(TimeOnly StartTime, TimeOnly EndTime)
    {
        return s => s.StartTime > s.EndTime && (
        StartTime<s.StartTime&&(EndTime<=s.EndTime ||EndTime>s.EndTime)||
        (StartTime>s.StartTime)&&(StartTime<s.EndTime&&StartTime<s.StartTime))||
        (StartTime<s.StartTime&&EndTime>s.StartTime&&EndTime>s.EndTime);
    }
    private Expression<Func<Shift, bool>> OverlappingDateExpression(DateOnly StartDate, DateOnly EndDate)
    {
        return s =>(s.StartDate>=StartDate && (EndDate>s.EndDate||EndDate<=s.EndDate))||
                   (s.StartDate < StartDate && (EndDate > s.EndDate || EndDate <= s.EndDate));
    }

   

    /// <summary>
    /// Builds an expression to detect overlaps between a non-midnight new shift and existing midnight shifts.
    /// </summary>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <param name="StartTime">Start time of the new shift.</param>
    /// <param name="EndTime">End time of the new shift.</param>
    /// <returns>Expression to find conflicting midnight shifts.</returns>
    private Expression<Func<Shift, bool>> IfNewNotOvernightOverlapsWithExistingMidnight(DateOnly StartDate, DateOnly EndDate, TimeOnly StartTime, TimeOnly EndTime)
    {
        Expression<Func<Shift, bool>> _overlappTimeExpression = OverlappingTimeMidnightShiftExpression(StartTime, EndTime);
        Expression<Func<Shift, bool>> _overlappDateExpression = OverlappingDateExpression(StartDate, EndDate);
        Expression<Func<Shift, bool>> expression = s => (!s.IsFullDay) && (s.StartTime > s.EndTime);
        return _overlappDateExpression.And(_overlappTimeExpression).And(expression);

    }

    /// <summary>
    /// Builds an expression to detect overlap between a new midnight shift and existing midnight shifts.
    /// </summary>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <param name="StartTime">Start time of the new shift.</param>
    /// <param name="EndTime">End time of the new shift.</param>
    /// <returns>Expression for midnight shift conflicts.</returns>
    private Expression<Func<Shift, bool>> IfNewOvernightOverlapsWithExistingMidnight(DateOnly StartDate, DateOnly EndDate, TimeOnly StartTime, TimeOnly EndTime)
    {
        Expression<Func<Shift, bool>> _overlappTimeExpression = OverlappingTimeMidnightShiftExpression(StartTime, EndTime);
        Expression<Func<Shift, bool>> _overlappDateExpression = OverlappingDateExpression(StartDate, EndDate);
        Expression<Func<Shift, bool>> expression = s => (!s.IsFullDay) && (s.StartTime > s.EndTime);
        return _overlappDateExpression.And(_overlappTimeExpression).And(expression);
    }

    /// <summary>
    /// Builds an expression to detect overlap between a new shift and existing full-day shifts.
    /// </summary>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <returns>Expression to find overlapping full-day shifts.</returns>
    private Expression<Func<Shift, bool>> AnExistingShiftIsFullDayOverlapsWithNewShift(DateOnly StartDate, DateOnly EndDate)
    {
        return s => s.IsFullDay && (
                      (StartDate >= s.StartDate && StartDate <= s.EndDate) ||
                      (EndDate >= s.StartDate && EndDate <= s.EndDate) ||
                      (StartDate <= s.StartDate && EndDate >= s.EndDate) ||
                      (StartDate >= s.StartDate && EndDate <= s.EndDate) ||
                      (StartDate == s.StartDate && EndDate == s.EndDate)
                  );
    }

    /// <summary>
    /// Builds an expression to detect whether a full-day new shift overlaps with any existing non-full-day shifts.
    /// </summary>
    /// <param name="IsFullday">Indicates whether the new shift is full-day (should be true here).</param>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <returns>Expression for overlapping full-day shift conflicts.</returns>
    private Expression<Func<Shift, bool>> CurrentShiftIsFullDayOverlapsWithExistingNotFullDayShift(bool IsFullday, DateOnly StartDate, DateOnly EndDate)
    {
        return s =>
               (StartDate >= s.StartDate && StartDate <= s.EndDate) ||
               (EndDate >= s.StartDate && EndDate <= s.EndDate) ||
               (StartDate <= s.StartDate && EndDate >= s.EndDate) ||
               (StartDate >= s.StartDate && EndDate <= s.EndDate);
    }

    /// <summary>
    /// Builds an expression to check for overlap between a non-midnight new shift and an existing non-midnight shift.
    /// </summary>
    /// <param name="StartTime">Start time of the new shift.</param>
    /// <param name="EndTime">End time of the new shift.</param>
    /// <returns>Expression to match non-midnight overlaps.</returns>
    private Expression<Func<Shift, bool>> NewShiftIsNotMidNightShift(TimeOnly StartTime, TimeOnly EndTime,DateOnly StartDate ,DateOnly EndDate)
    {
        Expression<Func<Shift, bool>> _overlappTimeExpression = OverlappingTimeNormalShiftExpression(StartTime, EndTime);
        Expression<Func<Shift, bool>> _overlappDateExpression = OverlappingDateExpression(StartDate, EndDate);
        Expression<Func<Shift, bool>> expression = s => (!s.IsFullDay) && (s.StartTime < s.EndTime);
        return _overlappDateExpression.And(_overlappTimeExpression).And(expression);
    }

    /// <summary>
    /// Checks if a new full-day shift overlaps with any existing non-full-day shifts.
    /// </summary>
    /// <param name="StartDate">Start date of the new shift.</param>
    /// <param name="EndDate">End date of the new shift.</param>
    /// <returns>Expression to match overlapping shifts.</returns>
    private Expression<Func<Shift, bool>> NewShiftFullDayOverlapsWithExistingShiftNotFullDay(DateOnly StartDate, DateOnly EndDate)
    {
        return s => (StartDate >= s.StartDate && StartDate <= s.EndDate) ||
                       (EndDate >= s.StartDate && EndDate <= s.EndDate) ||
                       (StartDate <= s.StartDate && EndDate >= s.EndDate) ||
                       (StartDate >= s.StartDate && EndDate <= s.EndDate) ||
                       (StartDate == s.StartDate && EndDate == s.EndDate);
    }

    /// <summary>
    /// Determines if a new shift that spans midnight overlaps with an existing midnight shift.
    /// </summary>
    /// <param name="StartTime">Start time of the new shift.</param>
    /// <param name="EndTime">End time of the new shift.</param>
    /// <returns>Expression to match overlapping midnight shifts.</returns>
    private Expression<Func<Shift, bool>> NewShiftIsMidNightShift(TimeOnly StartTime, TimeOnly EndTime,DateOnly StartDate , DateOnly EndDate)
    {
        Expression<Func<Shift, bool>> _overlappTimeExpression = OverlappingTimeMidnightShiftExpression(StartTime, EndTime);
        Expression<Func<Shift, bool>> _overlappDateExpression = OverlappingDateExpression(StartDate, EndDate);
        Expression<Func<Shift, bool>> expression = s => s.StartTime > s.EndTime;

        return  _overlappDateExpression.And(_overlappTimeExpression).And(expression);
    }

    /// <summary>
    /// Applies a search filter to a list of shifts based on basic fields like name, description, and garage name.
    /// </summary>
    /// <param name="input">Search input containing the search term.</param>
    /// <param name="queryable">The base queryable of ShiftDto.</param>
    /// <returns>Filtered queryable based on the search term.</returns>
    private IQueryable<ShiftDto> ApplySearchOrganization(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> queryable)
    {
        if (!input.SearchingTerm.IsNullOrEmpty())
        {
            var searchingItem = input.SearchingTerm.ToLower();
            queryable = queryable.Where(
                shift => shift.ArabicName.ToLower().Contains(searchingItem) ||
                         shift.EnglishName.ToLower().Contains(searchingItem) ||
                         shift.Description.ToLower().Contains(searchingItem) ||
                         shift.GarageArabicName.ToLower().Contains(searchingItem) ||
                         shift.GarageEnglishName.ToLower().Contains(searchingItem)
            );
        }
        return queryable;
    }

    /// <summary>
    /// Applies a search filter for MDKHL context, including organization and garage names.
    /// </summary>
    /// <param name="input">Search input containing the search term.</param>
    /// <param name="queryable">The base queryable of ShiftDto.</param>
    /// <returns>Filtered queryable based on the search term.</returns>
    private IQueryable<ShiftDto> ApplySearchMDKHL(PagedAndSortedAndSearchResultRequestDto input, IQueryable<ShiftDto> queryable)
    {
        if (!input.SearchingTerm.IsNullOrEmpty())
        {
            queryable = queryable.Where(
                shift => shift.OrganizationArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.OrganizationEnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.Description.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.GarageArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                         shift.GarageEnglishName.ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }
        return queryable;
    }
    /// <summary>
    /// Checks if any users are currently assigned to the shift.
    /// Throws a business exception if users are assigned.
    /// </summary>
    /// <param name="id">The shift ID to check.</param>
    private async Task CheckCountOfUsersAssignedToShift(int id)
    {
        var countOfUserAssignedToShift = await _shiftUserRepository.CountAsync(_ => _.ShiftId == id);
        if (countOfUserAssignedToShift > 0)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("Shift") + " " + LocalizationHelper.GetLocalizedString("DeleteValidation") + " " + LocalizationHelper.GetLocalizedString("Users"));
        }
    }

    /// <summary>
    /// Checks if a shift with the specified ID exists.
    /// Throws a business exception if not found.
    /// </summary>
    /// <param name="id">The shift ID to find.</param>
    /// <returns>The found Shift object or throws an exception if not found.</returns>
    private async Task<Shift?> IsFoundShift(int id)
    {
        var shift = await _repository.FirstOrDefaultAsync(_ => _.Id == id);
        if (shift == null)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("Shift") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
        }
        return shift;
    }
    /// <summary>
    /// Validates that both the garage and the organization exist for the given shift input.
    /// Throws exceptions if either is not found.
    /// </summary>
    /// <param name="input">The shift data containing the organization and garage IDs.</param>
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

    #endregion
}
