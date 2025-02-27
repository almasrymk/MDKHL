using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using SmartParking.KeeperShifts;
using SmartParking.Localization;
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
    private readonly IRepository<Shift, int> _shiftRepository;
    private readonly IRepository<KeeperShift, int> _keeperShiftRepository;

    private IDataFilter _dataFilter;

    public DailyShiftAppService(IRepository<DailyShift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository,
        IRepository<Garage, int> garageRepository, IMyUserRepository userRepository, IRepository<Shift, int> shiftRepository, IRepository<KeeperShift, int> keeperShiftRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageRepository = garageRepository;
        _userRepository = userRepository;
        _shiftRepository = shiftRepository;
        _keeperShiftRepository = keeperShiftRepository;
    }
    public override async Task<DailyShiftDto> CreateAsync(CreateUpdateDailyShiftDto input)
    {
        KeeperShift? assignShift = await GetAssignShift(input) ?? throw new Exception(LocalizationHelper.GetLocalizedString("NotAssignShift"));
        input.RequestTime = DateTime.Now;

        await (input.KeeperShiftRequestType switch
        {
            KeeperShiftRequestType.StartShift => HandleStartShift(input , assignShift),
            KeeperShiftRequestType.EndShift => HandleEndShift(input , assignShift),
            _ => throw new InvalidOperationException()
        });
        if (!input.TenantId.HasValue)
            input.TenantId = CurrentTenant.Id;
        return await base.CreateAsync(input);
    }
    public async Task<CreateUpdateDailyShiftDto> CreateDailyShiftDto(KeeperShiftDto input, KeeperShiftRequestType keeperShiftRequestType)
    {
        return new CreateUpdateDailyShiftDto() 
        { 
            GarageId = input.GarageId,
            KeeperId = input.KeeperId,
            ShiftId = input.ShiftId,
            KeeperShiftRequestType = keeperShiftRequestType,
            TenantId = input.TenantId,
            OrganizationId = input.OrganizationId,
            RequestTime = DateTime.Now
        };

    }
    private async Task<bool> HandleStartShift (CreateUpdateDailyShiftDto input ,   KeeperShift assignShift)
    {
        if(assignShift.EndTime < assignShift.StartTime)
        {
            CheckStartShiftMidNightDateAndTime(input, assignShift);
        }
        else
        {
            CheckStartShiftDateAndTime(input , assignShift);
        }

        return true;
    }

    private async Task<KeeperShift?> GetAssignShift(CreateUpdateDailyShiftDto input)
    {
        return await _keeperShiftRepository.FirstOrDefaultAsync(_ => _.KeeperId == input.KeeperId && _.ShiftId == input.ShiftId);
    }
    private void CheckStartShiftMidNightDateAndTime(CreateUpdateDailyShiftDto input, KeeperShift assignShift)
    {
        var currentTime = TimeOnly.FromDateTime(input.RequestTime);
        var currentDate = DateOnly.FromDateTime(input.RequestTime);
        if (currentDate < assignShift.StartDate || currentDate > assignShift.EndDate)
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));

        var isWithinShift = (currentTime >= assignShift.StartTime) || (currentTime < assignShift.EndTime);
        if(!isWithinShift)
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));
    }
    private void CheckStartShiftDateAndTime(CreateUpdateDailyShiftDto input , KeeperShift assignShift)
    {
        var currentTime = TimeOnly.FromDateTime(input.RequestTime);
        var currentDate = DateOnly.FromDateTime(input.RequestTime);
        if (currentDate < assignShift.StartDate || currentDate > assignShift.EndDate)
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));

        if (assignShift.StartTime < assignShift.EndTime  && currentTime < assignShift.StartTime || currentTime >= assignShift.EndTime)
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));
    }

    private async Task CheckIfAnotherKeeperInSameShift(KeeperShift assignShift, CreateUpdateDailyShiftDto request)
    {
        // EndShiftStartsInSameDay
        if (assignShift.StartTime<assignShift.EndTime)
        {
            await CheckIfKeeperInShiftInSameDay(request);
        }
        //midnightshift
        else
        {
            //sends request day before
            var result = await _repository.AnyAsync(e => e.ShiftId == request.ShiftId && e.RequestTime.Date.AddDays(-1) == request.RequestTime.Date && e.KeeperId != request.KeeperId && e.KeeperShiftRequestType == KeeperShiftRequestType.StartShift);
            if (!result)
            {
                ThrowCantEndShiftException();
            }
            await CheckIfKeeperInShiftInSameDay(request);
        }
    }

    private async Task CheckIfKeeperInShiftInSameDay(CreateUpdateDailyShiftDto request)
    {
        var result = await _repository.AnyAsync(e => e.ShiftId == request.ShiftId && e.RequestTime.Date == request.RequestTime.Date && e.KeeperId != request.KeeperId && e.KeeperShiftRequestType == KeeperShiftRequestType.StartShift);
        if (!result)
        {
            ThrowCantEndShiftException();
        }
        return;
    }

    private static void ThrowCantEndShiftException()
    {
        throw new Exception(LocalizationHelper.GetLocalizedString("CannotEndShift"));
    }

    private async Task<bool> HandleEndShift(CreateUpdateDailyShiftDto input , KeeperShift assignShift)
    {
        //check if keeper hasn't shift now.
        //if current shift is not midnight shift
        DailyShift dailyShift = new DailyShift();
        DailyShift nextDailyShift = new DailyShift();
        if (assignShift.StartTime < assignShift.EndTime)
            dailyShift = await HandleEndShiftInSameDay(input, dailyShift);
        //if current shift is midnight shift
        if (assignShift.StartTime > assignShift.EndTime)
            dailyShift = await HandleEndMidnightShift(input);

        //check if there is no keeper starts next shift before requested keeper requestes to end.
        //if next shift is not midnight shift
        //no Need to check if next shift is Midnight , keeper already will request to end in dame day :D .
            nextDailyShift = await NextShiftInSameDay(input, dailyShift);

        //check if shift time is appropriate to end
        await CheckIfAnotherKeeperInSameShift(assignShift,input);
        return true;
    }
    /// <summary>
    /// Method to check if keeper has started next shift.
    /// </summary>
    /// <param name="input">CreationRequestDto</param>
    /// <param name="dailyShift">ShiftThatKeeperHasBeenAssigned</param>
    /// <param name="nextDailyShift"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<DailyShift> NextShiftInSameDay(CreateUpdateDailyShiftDto input, DailyShift dailyShift)
    {
        var nextDailyShift = await _repository.FirstOrDefaultAsync(_ => _.ShiftId == input.ShiftId && _.KeeperId != input.KeeperId && _.RequestTime > dailyShift.RequestTime && _.KeeperShiftRequestType == KeeperShiftRequestType.StartShift);
        if (nextDailyShift is null)
        {
            ThrowCantEndShiftException();
            return null;
        }

        return nextDailyShift;
    }

    /// <summary>
    /// Method to check if keeper has already sent to start request in day before request.
    /// </summary>
    /// <param name="input">CreationRequestDto</param>
    /// <param name="dailyShift">ShiftThatKeeperHasBeenAssigned</param>
    /// <returns>RequestToStartShift</returns>
    /// <exception cref="Exception">IfKeeperCan'tEndShift</exception>
    private async Task<DailyShift> HandleEndMidnightShift(CreateUpdateDailyShiftDto input)
    {
        var dailyShift = await _repository.FirstOrDefaultAsync(_ => _.ShiftId == input.ShiftId && _.KeeperId == input.KeeperId && (_.RequestTime.Day == (input.RequestTime.Day - 1)) && _.KeeperShiftRequestType == KeeperShiftRequestType.StartShift);
        if (dailyShift is null)
        {

            ThrowCantEndShiftException();
            return null;
        }

        return dailyShift;
    }
    /// <summary>
    /// Method to check if keeper has already sent to start request in same day.
    /// </summary>
    /// <param name="input">CreationRequestDto</param>
    /// <param name="dailyShift">ShiftThatKeeperHasBeenAssigned</param>
    /// <returns>RequestToStartShift</returns>
    /// <exception cref="Exception">IfKeeperCan'tEndShift</exception>
    private async Task<DailyShift> HandleEndShiftInSameDay(CreateUpdateDailyShiftDto input, DailyShift dailyShift)
    {
        dailyShift = await _repository.FirstOrDefaultAsync(_ => _.ShiftId == input.ShiftId && _.KeeperId == input.KeeperId && _.RequestTime.Date == DateTime.Now.Date);
        if (dailyShift is null)
        {

            ThrowCantEndShiftException();
            return null;
        }

        return dailyShift;
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
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
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
                         join keeperShift in queryKeeperShift
                         on dailyShift.ShiftId equals keeperShift.ShiftId
                         join shift in queryShift
                         on dailyShift.ShiftId equals shift.Id
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
                             ShiftId = dailyShift.ShiftId,
                             ShiftNameAr = shift.ArabicName,
                             ShiftNameEn = shift.EnglishName,
                             StartDate = keeperShift.StartDate,
                             EndDate = keeperShift.EndDate,
                             StartTime = keeperShift.StartTime,
                             EndTime = keeperShift.EndTime
                         };
            return await result.FirstOrDefaultAsync();
        }
        
    }
    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsAsync (PagedAndSortedAndSearchResultRequestDto input ,Guid?  TenantId ,Guid? userId )
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var result = from dailyShift in queryDailyShift
                         where
                         TenantId.HasValue ? dailyShift.TenantId == TenantId : true 
                         && userId.HasValue ? dailyShift.KeeperId == userId : true 
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join organization in queryOrganization
                         on dailyShift.OrganizationId equals organization.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id
                         join keeperShift in queryKeeperShift
                         on dailyShift.ShiftId equals keeperShift.ShiftId
                         join shift in queryShift
                         on dailyShift.ShiftId equals shift.Id
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
                             ShiftId = dailyShift.ShiftId,
                             ShiftNameAr = shift.ArabicName , 
                             ShiftNameEn = shift.EnglishName ,
                             StartDate = keeperShift.StartDate,
                             EndDate = keeperShift.EndDate,
                             StartTime = keeperShift.StartTime,
                             EndTime = keeperShift.EndTime
                             
                         };
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            result = ApplySorting(input, result);
            result = await ApplySearch(input, result);
            var total = result.Count();
            return new PagedResultDto<DailyShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }

    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? TenantId , Guid? userId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            if (userId.HasValue)
                queryDailyShift = queryDailyShift.Where(_ => _.KeeperId == userId);
           
            var result = from dailyShift in queryDailyShift
                         where TenantId.HasValue ? dailyShift.TenantId == TenantId : true
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id
                         join keeperShift in queryKeeperShift
                         on dailyShift.ShiftId equals keeperShift.ShiftId
                         join shift in queryShift
                         on dailyShift.ShiftId equals shift.Id

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
                             ShiftId = dailyShift.ShiftId,
                             ShiftNameAr = shift.ArabicName,
                             ShiftNameEn = shift.EnglishName,
                             StartDate = keeperShift.StartDate,
                             EndDate = keeperShift.EndDate,
                             StartTime = keeperShift.StartTime,
                             EndTime = keeperShift.EndTime
                         };

            result = ApplySorting(input, result);
            result = await ApplySearch(input, result);
            var total = await result.CountAsync();
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            return new PagedResultDto<DailyShiftDto>
            {
                Items = result.ToList(), 
                TotalCount = total
            };
        }
    }
    private static IQueryable<DailyShiftDto>? ApplySorting(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DailyShiftDto>? result)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting) && sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }
        result = result.OrderBy(sorting);
        return result;
    }

    private async Task<IQueryable<DailyShiftDto>?> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DailyShiftDto>? queryable)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
            {

                queryable = queryable.Where(or =>
                or.KeeperName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.GarageNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.GarageNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.OrganizationNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.KeeperShiftRequestType.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.ShiftNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.ShiftNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.RequestTime.ToString().Contains(input.SearchingTerm)
            );

            }
            await Task.Yield();
            return queryable;
        }
        
    }
    
}
