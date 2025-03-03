using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Shifts;
using SmartParking.Enums;
using SmartParking.Localization;
using SmartParking.Repositories;
using SmartParking.Results;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace SmartParking.KeeperShifts;

public class KeeperShiftsAppService : ApplicationService, IKeeperShiftsAppService
{
    private readonly IRepository<KeeperShift,int> _keeperShiftRepository;
    private readonly IRepository<Garage,int> _garageRepository;
    private readonly IRepository<Organization,int> _organizationRepository;
    private readonly IRepository<Shift,int> _shiftRepository;
    private readonly IMyUserRepository _userRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IMapper _mapper;

    public KeeperShiftsAppService(IRepository<KeeperShift, int> repository
       ,  IRepository<Garage, int> garagerepository, IRepository<Organization, int> orgrepository,
       IRepository<Shift, int> shiftrepository, IMyUserRepository userrepository,
        IDataFilter dataFilter, IMapper mapper)

    {
        _keeperShiftRepository = repository;
        _garageRepository = garagerepository;
        _organizationRepository = orgrepository;
        _shiftRepository = shiftrepository;
        _userRepository = userrepository;
        _dataFilter = dataFilter;
        _mapper = mapper;
    }

    public async Task<bool> CreateAsync(CreateUpdateKeeperShiftDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var shift = await _shiftRepository.FirstOrDefaultAsync(s => s.Id == input.ShiftId);
            if (shift == null)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("Shifts") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            CheckShiftStatus(shift);
            await CheckDupplicateShift(input);
            ValidateDate(input, shift);
            await OverlappingValidation(input, shift);
            var keeperShift = _mapper.Map<KeeperShift>(input);
            keeperShift.OrganizationId = shift.OrganizationId;
            keeperShift.GarageId = shift.GarageId;
            keeperShift.StartTime = shift.StartTime;
            keeperShift.EndTime = shift.EndTime;
            await _keeperShiftRepository.InsertAsync(keeperShift, true);
            return true;
        }
    }

    private async Task OverlappingValidation(CreateUpdateKeeperShiftDto input, Shift? shift)
    {
        bool isOverlapping = await _keeperShiftRepository.AnyAsync(ks =>
             ks.KeeperId == input.KeeperId &&
             ks.ShiftId != input.ShiftId &&
             (
                 (shift.StartDate <= ks.EndDate && shift.EndDate >= ks.StartDate)
             ) 
             &&
             (
                 (shift.StartTime < ks.EndTime && shift.EndTime > ks.StartTime)
             )
        );

        if (isOverlapping)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("Shift overlaps with an existing shift."));
        }
    }


    private static void CheckShiftStatus(Shift? shift)
    {
        if (shift.Status != Status.Active)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("ShiftNotActive"));
        }
    }

    private static void ValidateDate(CreateUpdateKeeperShiftDto input, Shift shift)
    {
        if (input.StartDate == default || input.EndDate == default)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        }

        if (input.EndDate < input.StartDate)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("EndDateCannotBeBeforeStartDate"));
        }

        if (input.StartDate < shift.StartDate || input.EndDate > shift.EndDate)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("ShiftDateOutOfRange"));
        }
        
    }


    private async Task CheckDupplicateShift(CreateUpdateKeeperShiftDto input)
    {
        bool alreadyAssigned = await _keeperShiftRepository.AnyAsync(ks =>
                     ks.KeeperId == input.KeeperId &&
                     ks.ShiftId == input.ShiftId);

        if (alreadyAssigned)
        {
            throw new BusinessException(LocalizationHelper.GetLocalizedString("KeeperAlreadyAssignedToShift"));
        }
    }


  

    public async Task<PagedResultDto<KeeperShiftDto>> GetMyShiftsForCurrentUser(Guid? userId, PagedAndSortedAndSearchResultRequestDto input)
    {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var currentTime = TimeOnly.FromDateTime(DateTime.Now);
            var currentDateTime = DateTime.Now; 

            var result = from keepershift in queryKeeperShift
                         where userId.HasValue ? keepershift.KeeperId == userId : true
                         orderby
                            keepershift.StartDate <= currentDate
                            && keepershift.EndDate >= currentDate descending , 
                             keepershift.StartTime <= currentTime
                            && keepershift.EndTime >= currentTime descending ,
                            keepershift.StartTime > currentTime descending
                         join garage in queryGarage
                         on keepershift.GarageId equals garage.Id
                         join user in queryUser
                         on keepershift.KeeperId equals user.Id
                         join shift in queryShift
                         on keepershift.ShiftId equals shift.Id
                         select new KeeperShiftDto
                         {
                             Id = keepershift.Id,
                             KeeperId = keepershift.KeeperId,
                             KeeperName = user.Name,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             OrganizationId = keepershift.OrganizationId,
                             OrganizationNameAr = string.Empty,
                             OrganizationNameEn = string.Empty,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime , 
                             ArabicName = shift.ArabicName,
                             EnglishName = shift.EnglishName,
                             TenantId = keepershift.TenantId, 
                             IsFullDay = keepershift.IsFullDay

                         };

            result = ApplySearch(input, result);

            var total = result.Count();
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            return new PagedResultDto<KeeperShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        
    }

    public async Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForMdkhal(Guid? tenantId, Guid? userId , PagedAndSortedAndSearchResultRequestDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            if (userId.HasValue)
                queryKeeperShift = queryKeeperShift.Where(x => x.KeeperId == userId);
            var result = from keepershift in queryKeeperShift
                         where
                         tenantId.HasValue ? keepershift.TenantId == tenantId : true
                         join garage in queryGarage
                         on keepershift.GarageId equals garage.Id
                         join user in queryUser
                         on keepershift.KeeperId equals user.Id
                         join org in queryOrganization
                         on keepershift.OrganizationId equals org.Id
                         join shift in queryShift
                         on keepershift.ShiftId equals shift.Id
                         select new KeeperShiftDto
                         {
                             Id = keepershift.Id,
                             KeeperId = keepershift.KeeperId,
                             KeeperName = user.Name,
                             OrganizationId = keepershift.OrganizationId,
                             OrganizationNameAr = org.ArabicName, 
                             OrganizationNameEn =  org.EnglishName, 
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime,
                             ArabicName = shift.ArabicName,
                             EnglishName = shift.EnglishName
                         };
            
            result =  ApplySearch(input, result);
            result = ApplySorting(input, result);
            var total = result.Count();
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            return new PagedResultDto<KeeperShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }
    private IQueryable<KeeperShiftDto> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<KeeperShiftDto>? queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
            or.KeeperName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.GarageNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.GarageNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.OrganizationNameAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.OrganizationNameEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
            or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower())

        );

        }
            return queryable;
        

    }

    public async Task<PagedResultDto<KeeperShiftDto>> GetListAssignedShiftsToUserForOrganization(Guid? tenantId , Guid? userId , PagedAndSortedAndSearchResultRequestDto input )
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            if(userId.HasValue)
                queryKeeperShift = queryKeeperShift.Where(x => x.KeeperId == userId);

            var result = from keepershift in queryKeeperShift
                         where
                         tenantId == null ? keepershift.TenantId == tenantId : true
                         join garage in queryGarage
                         on keepershift.GarageId equals garage.Id
                         join user in queryUser
                         on keepershift.KeeperId equals user.Id
                         join shift in queryShift
                         on keepershift.ShiftId equals shift.Id
                         select new KeeperShiftDto
                         {
                             Id = keepershift.Id,
                             KeeperId = keepershift.KeeperId,
                             KeeperName = user.Name,
                             OrganizationId = keepershift.OrganizationId,
                             OrganizationNameAr = string.Empty, 
                             OrganizationNameEn = string.Empty, 
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime,
                             ArabicName = shift.ArabicName,
                             EnglishName = shift.EnglishName
                         };
            
            result = ApplySearch(input, result);
            result = ApplySorting(input, result);
            var total = result.Count();
            result = result.Skip(input.SkipCount).Take(input.MaxResultCount);
            return new PagedResultDto<KeeperShiftDto>
            {
                TotalCount = total,
                Items = result.ToList()
            };
        }
    }

    private static IQueryable<KeeperShiftDto> ApplySorting(PagedAndSortedResultRequestDto input,  IQueryable<KeeperShiftDto> result)
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
    private async Task<KeeperShift> GetShiftName(KeeperShift result)
    {
       
        var queryShift = await _shiftRepository.GetQueryableAsync();
        var shift = await queryShift.FirstOrDefaultAsync(x => x.Id == result.ShiftId);
        if (shift != null)
        {
            result.ArabicName = shift.ArabicName;
            result.EnglishName = shift.EnglishName;
        }
        return result;
    }
    public async Task<KeeperShiftDto?> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryKeeperShift = await _keeperShiftRepository.GetQueryableAsync();
            var queryGarage = await _garageRepository.GetQueryableAsync();
            var queryOrganization = await _organizationRepository.GetQueryableAsync();
            var queryUser = await _userRepository.GetQueryableAsync();
            var queryShift = await _shiftRepository.GetQueryableAsync();
            var IsFoundKeeperShift = await _keeperShiftRepository.FirstOrDefaultAsync(_ => _.Id == id);
            if (IsFoundKeeperShift is null)
                throw new Exception("NotFoundValidation");
            var result = from keepershift in queryKeeperShift
                         where keepershift.Id == id
                         join garage in queryGarage
                         on keepershift.GarageId equals garage.Id
                         join user in queryUser
                         on keepershift.KeeperId equals user.Id
                         join shift in queryShift
                         on keepershift.ShiftId equals shift.Id

                         select new KeeperShiftDto
                         {
                             Id = keepershift.Id,
                             KeeperId = keepershift.KeeperId,
                             KeeperName = user.Name,
                             OrganizationId = keepershift.OrganizationId,
                             GarageNameAr = garage.NameAr,
                             GarageNameEn = garage.NameEn,
                             GarageId = keepershift.GarageId,
                             ShiftId = keepershift.ShiftId,
                             StartDate = keepershift.StartDate,
                             EndDate = keepershift.EndDate,
                             StartTime = keepershift.StartTime,
                             EndTime = keepershift.EndTime,
                             ArabicName = shift.ArabicName,
                             EnglishName = shift.EnglishName
                             
                         };
            return await result.FirstOrDefaultAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var keeperShift = await _keeperShiftRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (keeperShift is null)
            {
                throw new BusinessException(LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }

            if( ! await _keeperShiftRepository.AnyAsync(s => s.KeeperId != keeperShift.KeeperId && s.ShiftId == keeperShift.Id))
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("RemoveAssignShiftValidation"));
            }

            await _keeperShiftRepository.HardDeleteAsync(keeperShift, true);
        }
    }

    public async Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var users = await _userRepository.FindUserShiftsOrganizationAsync(input.SkipCount,input.MaxResultCount,input.Sorting,input.SearchingTerm);
        return new PagedResultDto<UserWithRole>
        {
            TotalCount = users.PageSize,
            Items = users.Queryable.ToList()
        };
    } 
    
    public async Task<PagedResultDto<UserWithRole>> GetListUserCanAssignShiftsMdkhlAsync(Guid? tenantId,PagedAndSortedAndSearchResultRequestDto input)
    {

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var users = await _userRepository.FindUserShiftsMdkhlAsync(input.SkipCount, input.MaxResultCount, input.Sorting, input.SearchingTerm, tenantId);
            return new PagedResultDto<UserWithRole>
            {
                TotalCount = users.PageSize,
                Items = users.Queryable.ToList()
            };
        }
    }

    private static bool BuildExperssionSearchInUser(PagedAndSortedAndSearchResultRequestDto input, IdentityUser user, IdentityRole role)
    {
        return !input.SearchingTerm.IsNullOrWhiteSpace() ?
                                    user.Name.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || role.Name.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.PhoneNumber.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.UserName.ToLower().Contains(input.SearchingTerm.ToLower())
                                    || user.Email.ToLower().Contains(input.SearchingTerm.ToLower())
                                    : true;
    }

    private static IQueryable<UserWithRole> ApplySortingForUser(PagedAndSortedResultRequestDto input, IQueryable<UserWithRole> result)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting) && sorting.Contains("DESC"))
        {
            var sortingSplit = sorting.Split(' ').ToList();
            sorting = sortingSplit[0] + " descending";
        }

        result = result.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);
        return result;
    }

}
