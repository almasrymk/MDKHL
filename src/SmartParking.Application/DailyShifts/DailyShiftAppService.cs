using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Payments;
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
/// <summary>
/// Service to handle DailyShift
/// </summary>
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
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private IDataFilter _dataFilter;

    public DailyShiftAppService(IRepository<DailyShift, int> repository, IDataFilter dataFilter, IRepository<Organization, int> organizationRepository,
        IRepository<Garage, int> garageRepository, IMyUserRepository userRepository, IRepository<Shift, int> shiftRepository, IRepository<KeeperShift, int> keeperShiftRepository, IRepository<Payment, Guid> paymentRepository) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
        _organizationRepository = organizationRepository;
        _garageRepository = garageRepository;
        _userRepository = userRepository;
        _shiftRepository = shiftRepository;
        _keeperShiftRepository = keeperShiftRepository;
        _paymentRepository = paymentRepository;
    }
    #region public methods 
    /// <inheritdoc/>
    public override async Task<DailyShiftDto> CreateAsync(CreateUpdateDailyShiftDto input)
    {
        KeeperShift? assignShift = await GetAssignShift(input) ?? throw new Exception(LocalizationHelper.GetLocalizedString("NotAssignShift"));
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
    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public override async Task<DailyShiftDto> UpdateAsync(int id, CreateUpdateDailyShiftDto input)
    {
        return await base.UpdateAsync(id, input);
    }
    /// <inheritdoc/>>
    public override Task DeleteAsync(int id)
    {
        return base.DeleteAsync(id);
    }
    /// <inheritdoc/>
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
            var queryPayment = await _paymentRepository.GetQueryableAsync();

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
                         on new { dailyShift.KeeperId, dailyShift.ShiftId }
                         equals new { keeperShift.KeeperId, keeperShift.ShiftId }
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
                             EndTime = keeperShift.EndTime ,
                             AmountOfCash = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where
                                                                                                                    (x => x.PaymentMethod == PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime
                                                                                                                    )
                                                                                                                    .Sum(_ => _.Amount) : 0,
                             AmountOfVisa = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where
                                                                                                                    (x => x.PaymentMethod != PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime)
                                                                                                                    .Sum(_ => _.Amount) : 0

                         };
            return await result.FirstOrDefaultAsync();
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? TenantId, Guid? userId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryDailyShift = await _repository.GetQueryableAsync();
            var queryGarge = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryPayment = await _paymentRepository.GetQueryableAsync();
            if (userId.HasValue)
                queryDailyShift = queryDailyShift.Where(_ => _.KeeperId == userId);
            var result = from dailyShift in queryDailyShift
                         where
                         TenantId.HasValue ? dailyShift.TenantId == TenantId : true
                         join user in queryUser
                         on dailyShift.KeeperId equals user.Id
                         join organization in queryOrganization
                         on dailyShift.OrganizationId equals organization.Id
                         join garage in queryGarge
                         on dailyShift.GarageId equals garage.Id
                         join keeperShift in queryKeeperShift
                         on new { dailyShift.KeeperId, dailyShift.ShiftId }
                         equals new { keeperShift.KeeperId, keeperShift.ShiftId }
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
                             EndTime = keeperShift.EndTime,
                             AmountOfCash = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where
                                                                                                                    (x=> x.PaymentMethod == PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime
                                                                                                                    )
                                                                                                                    .Sum(_ => _.Amount) : 0,
                             AmountOfVisa = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where 
                                                                                                                    (x => x.PaymentMethod != PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime)
                                                                                                                    .Sum(_ => _.Amount) : 0,

                         };
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);

            result = ApplySorting(input, result);
            result = await ApplySearch(input, result);

            return new PagedResultDto<DailyShiftDto>
            {
                TotalCount = result.Count(),
                Items = result.ToList()
            };
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? TenantId, Guid? userId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryPayment = await _paymentRepository.GetQueryableAsync();
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
                         on new { dailyShift.KeeperId, dailyShift.ShiftId }
                         equals new { keeperShift.KeeperId, keeperShift.ShiftId }
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
                             EndTime = keeperShift.EndTime ,
                             AmountOfCash = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where
                                                                                                                    (x => x.PaymentMethod == PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime
                                                                                                                    )
                                                                                                                    .Sum(_ => _.Amount) : 0,
                             AmountOfVisa = dailyShift.KeeperShiftRequestType == KeeperShiftRequestType.EndShift ? queryPayment.Where
                                                                                                                    (x => x.PaymentMethod != PaymentMethod.Cash
                                                                                                                    && x.CreatorId == dailyShift.KeeperId
                                                                                                                    && x.CreationTime >= queryDailyShift.Where(_ => _.ShiftId == dailyShift.ShiftId && _.KeeperId == dailyShift.KeeperId && _.RequestTime.Date == dailyShift.RequestTime.Date && _.GarageId == dailyShift.GarageId && _.OrganizationId == dailyShift.OrganizationId).Select(_ => _.RequestTime).FirstOrDefault()
                                                                                                                    && x.CreationTime <= dailyShift.RequestTime)
                                                                                                                    .Sum(_ => _.Amount) : 0,

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
    #endregion

    #region Private Methods
    /// <summary>
    /// Handles logic for starting a keeper's shift based on the provided input and assigned shift.
    /// Determines whether the shift spans midnight or not, and checks the appropriate timing constraints.
    /// </summary>
    /// <param name="input">The shift start request data.</param>
    /// <param name="assignShift">The assigned shift configuration for the keeper.</param>
    /// <returns>A task that returns <c>true</c> if the validation passes; otherwise, an exception is thrown.</returns>
    /// <exception cref="Exception">Thrown if the shift cannot be started due to time/date constraints.</exception>
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
    /// <summary>
    /// Retrieves the assigned shift for a specific keeper and shift ID.
    /// </summary>
    /// <param name="input">The shift input containing the keeper and shift IDs.</param>
    /// <returns>The assigned <see cref="KeeperShift"/> if found; otherwise, <c>null</c>.</returns>
    private async Task<KeeperShift?> GetAssignShift(CreateUpdateDailyShiftDto input)
    {
        return await _keeperShiftRepository.FirstOrDefaultAsync(_ => _.KeeperId == input.KeeperId && _.ShiftId == input.ShiftId);
    }
    /// <summary>
    /// Validates whether a shift that spans over midnight can be started based on the request time.
    /// </summary>
    /// <param name="input">The shift request data.</param>
    /// <param name="assignShift">The assigned shift that spans over midnight.</param>
    /// <exception cref="Exception">Thrown if the request is outside the allowed date or time window.</exception>
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
    /// <summary>
    /// Validates whether a regular (same-day) shift can be started based on the request time.
    /// </summary>
    /// <param name="input">The shift request data.</param>
    /// <param name="assignShift">The assigned shift details.</param>
    /// <exception cref="Exception">Thrown if the request is outside the shift's start and end date/time.</exception>
    private void CheckStartShiftDateAndTime(CreateUpdateDailyShiftDto input , KeeperShift assignShift)
    {
        var currentTime = TimeOnly.FromDateTime(input.RequestTime);
        var currentDate = DateOnly.FromDateTime(input.RequestTime);
        if (!(currentDate >= assignShift.StartDate && currentDate <= assignShift.EndDate))
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));

        if (assignShift.StartTime < assignShift.EndTime  && currentTime < assignShift.StartTime || currentTime >= assignShift.EndTime)
            throw new Exception(LocalizationHelper.GetLocalizedString("CannotStartShift"));
    }
    /// <summary>
    /// Validates if there is already a keeper who started the shift on the same day and organization/garage.
    /// </summary>
    /// <param name="request">The shift request data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown if no other keeper has started the shift.</exception>
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
            using (_dataFilter.Disable<IMultiTenant>())
            {
                //sends request day before
                var result = await _repository.AnyAsync(e => e.ShiftId == request.ShiftId && e.RequestTime.Date == request.RequestTime.Date.AddDays(-1) && e.KeeperId != request.KeeperId && e.KeeperShiftRequestType == KeeperShiftRequestType.StartShift && e.OrganizationId == request.OrganizationId && e.GarageId == request.GarageId);
                if (!result)
                {
                    await CheckIfKeeperInShiftInSameDay(request);
                }
            }
        }
    }
    /// <summary>
    /// Validates if there is already a keeper who started the shift on the same day and organization/garage.
    /// </summary>
    /// <param name="request">The shift request data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown if no other keeper has started the shift.</exception>
    private async Task CheckIfKeeperInShiftInSameDay(CreateUpdateDailyShiftDto request)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var result = await _repository.AnyAsync(e => e.ShiftId == request.ShiftId && e.RequestTime.Date == request.RequestTime.Date && e.KeeperId != request.KeeperId && e.KeeperShiftRequestType == KeeperShiftRequestType.StartShift && e.OrganizationId == request.OrganizationId && e.GarageId == request.GarageId);
            if (!result)
            {
                ThrowCantEndShiftException();
            }
            return;
        }
    }
    /// <summary>
    /// Throws a localized exception indicating the shift cannot be ended.
    /// </summary>
    /// <exception cref="Exception">Always thrown with a localized "CannotEndShift" message.</exception>
    private static void ThrowCantEndShiftException()
    {
        throw new Exception(LocalizationHelper.GetLocalizedString("CannotEndShift"));
    }
    /// <summary>
    /// Handles the logic for ending a keeper's shift, taking into account regular and midnight shifts.
    /// </summary>
    /// <param name="input">The input data for ending the shift, including request time and keeper/shift identifiers.</param>
    /// <param name="assignShift">The shift assigned to the keeper.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <c>true</c> if the shift end was handled successfully.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if there are conflicts or invalid timing for ending the shift (e.g., no valid start request found).
    /// </exception>
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
        //nextDailyShift = await NextShiftInSameDay(input, dailyShift);

        //check if shift time is appropriate to end
        await CheckIfAnotherKeeperInSameShift(assignShift,input);
        return true;
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
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var dailyShift = await _repository.FirstOrDefaultAsync(_ => _.ShiftId == input.ShiftId && _.KeeperId == input.KeeperId && (_.RequestTime.DayOfYear == (input.RequestTime.DayOfYear - 1)) && _.KeeperShiftRequestType == KeeperShiftRequestType.StartShift&&_.OrganizationId==input.OrganizationId && _.GarageId==input.GarageId);
            if (dailyShift is null)
            {
                ThrowCantEndShiftException();
                return null;
            }

            return dailyShift;
        }
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
        using (_dataFilter.Disable<IMultiTenant>())
        {
            dailyShift = await _repository.FirstOrDefaultAsync(_ => _.ShiftId == input.ShiftId && _.KeeperId == input.KeeperId && _.RequestTime.Date == input.RequestTime.Date &&_.GarageId==input.GarageId && _.OrganizationId==input.OrganizationId);
            if (dailyShift is null)
            {

                ThrowCantEndShiftException();
                return null;
            }

            return dailyShift;
        }
    }
    /// <summary>
    /// Applies sorting to the given query based on the provided sorting string in the input.
    /// </summary>
    /// <param name="input">The request input containing the sorting parameter.</param>
    /// <param name="result">The queryable list of <see cref="DailyShiftDto"/> to apply sorting on.</param>
    /// <returns>The sorted <see cref="IQueryable{DailyShiftDto}"/> or the original if no sorting is applied.</returns>
    private static IQueryable<DailyShiftDto>? ApplySorting(PagedAndSortedAndSearchResultRequestDto input, IQueryable<DailyShiftDto>? result)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting))
        {
            if (sorting.Contains("DESC"))
            {
                var sortingSplit = sorting.Split(' ').ToList();
                sorting = sortingSplit[0] + " descending";
            }
            result = result.OrderBy(sorting);
        }
        return result;
    }

    /// <summary>
    /// Applies a search filter to the given query based on the provided search term in the input.
    /// </summary>
    /// <param name="input">The request input containing the search term.</param>
    /// <param name="queryable">The queryable list of <see cref="DailyShiftDto"/> to apply search filtering on.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the filtered <see cref="IQueryable{DailyShiftDto}"/> result.
    /// </returns>
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
                    or.OrganizationNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.KeeperShiftRequestType.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.ShiftNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.ShiftNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                    or.RequestTime.ToString().Contains(input.SearchingTerm)
                );
            }
            await Task.Yield(); // Placeholder to keep the method signature async
            return queryable;
        }
    }

    #endregion
    #region UnUsed Methods 
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
    #endregion
}
